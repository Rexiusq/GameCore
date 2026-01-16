using GameCore.Interfaces;
using GameCore.Enums;
using GameCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Managers
{
    /// <summary>
    /// Tur yöneticisi
    /// - Single Responsibility: Sadece tur mantığını yönetir
    /// - Sıra tabanlı tüm oyunlarda kullanılabilir
    /// - Circular iteration (son oyuncudan sonra ilk oyuncuya döner)
    /// </summary>
    public class TurnManager : ITurnManager
    {
        private readonly List<IPlayer> _playerOrder;
        private int _currentPlayerIndex;
        private readonly List<Turn> _turnHistory;

        public IPlayer CurrentPlayer => _playerOrder[_currentPlayerIndex];
        public int CurrentTurnNumber { get; private set; }
        public TurnStatus TurnStatus { get; private set; }

        /// <summary>Tur geçmişini okuma amaçlı döndürür</summary>
        public IReadOnlyList<Turn> TurnHistory => _turnHistory.AsReadOnly();

        public TurnManager(IReadOnlyList<IPlayer> players)
        {
            if (players == null || players.Count == 0)
                throw new ArgumentException("Player list cannot be empty", nameof(players));

            // Sadece aktif oyuncuları al
            _playerOrder = players
                .Where(p => p.Status == PlayerStatus.Active || p.Status == PlayerStatus.Waiting)
                .ToList();

            if (_playerOrder.Count == 0)
                throw new InvalidOperationException("No active players found");

            _currentPlayerIndex = 0;
            CurrentTurnNumber = 0;
            TurnStatus = TurnStatus.Pending;
            _turnHistory = new List<Turn>();
        }

        public void StartTurn()
        {
            if (TurnStatus == TurnStatus.Active)
                throw new InvalidOperationException("Turn already started");

            CurrentTurnNumber++;
            TurnStatus = TurnStatus.Active;

            var turn = new Turn(CurrentTurnNumber, CurrentPlayer.Id);
            _turnHistory.Add(turn);
        }

        public void EndTurn()
        {
            if (TurnStatus != TurnStatus.Active)
                throw new InvalidOperationException("No active turn to end");

            var currentTurn = _turnHistory.Last();
            currentTurn.Complete();
            TurnStatus = TurnStatus.Completed;
        }

        public void NextTurn()
        {
            // Mevcut turu bitir (eğer aktifse)
            if (TurnStatus == TurnStatus.Active)
                EndTurn();

            // Bir sonraki oyuncuya geç (circular)
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _playerOrder.Count;
            
            // Elenmemiş oyuncuyu bul
            int attempts = 0;
            while (_playerOrder[_currentPlayerIndex].Status == PlayerStatus.Eliminated 
                   && attempts < _playerOrder.Count)
            {
                _currentPlayerIndex = (_currentPlayerIndex + 1) % _playerOrder.Count;
                attempts++;
            }

            if (attempts >= _playerOrder.Count)
                throw new InvalidOperationException("No active players remaining");

            TurnStatus = TurnStatus.Pending;
        }

        public bool IsPlayerTurn(string playerId)
        {
            return CurrentPlayer.Id == playerId && TurnStatus == TurnStatus.Active;
        }

        /// <summary>
        /// Belirli bir oyuncuyu sıradan çıkarır (elendi)
        /// </summary>
        public void EliminatePlayer(string playerId)
        {
            var player = _playerOrder.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.Status = PlayerStatus.Eliminated;
            }
        }

        /// <summary>
        /// Sıra düzenini değiştir (bazı oyunlar için gerekebilir)
        /// </summary>
        public void ShuffleOrder()
        {
            var random = new Random();
            var shuffled = _playerOrder.OrderBy(_ => random.Next()).ToList();
            _playerOrder.Clear();
            _playerOrder.AddRange(shuffled);
            _currentPlayerIndex = 0;
        }
    }

    /// <summary>
    /// Oyuncu yöneticisi
    /// - Oyuncu koleksiyonunu yönetir
    /// - Oyuncu durumlarını takip eder
    /// - Single Responsibility
    /// </summary>
    public class PlayerManager
    {
        private readonly Dictionary<string, IPlayer> _players;

        public IReadOnlyList<IPlayer> AllPlayers => _players.Values.ToList().AsReadOnly();
        public IReadOnlyList<IPlayer> ActivePlayers => 
            _players.Values.Where(p => p.Status == PlayerStatus.Active).ToList().AsReadOnly();

        public int PlayerCount => _players.Count;
        public int ActivePlayerCount => ActivePlayers.Count;

        public PlayerManager()
        {
            _players = new Dictionary<string, IPlayer>();
        }

        public void AddPlayer(IPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (_players.ContainsKey(player.Id))
                throw new InvalidOperationException($"Player {player.Id} already exists");

            _players[player.Id] = player;
        }

        public void RemovePlayer(string playerId)
        {
            if (_players.ContainsKey(playerId))
            {
                _players.Remove(playerId);
            }
        }

        public IPlayer? GetPlayer(string playerId)  // DÜZELTME: nullable eklendi
        {
            return _players.TryGetValue(playerId, out var player) ? player : null;
        }

        public bool HasPlayer(string playerId)
        {
            return _players.ContainsKey(playerId);
        }

        public void UpdatePlayerStatus(string playerId, PlayerStatus status)
        {
            if (_players.TryGetValue(playerId, out var player))
            {
                player.Status = status;
            }
        }

        /// <summary>
        /// Tüm oyuncuları belirli bir duruma getirir
        /// </summary>
        public void SetAllPlayersStatus(PlayerStatus status)
        {
            foreach (var player in _players.Values)
            {
                player.Status = status;
            }
        }
    }

    /// <summary>
    /// Event/Action dağıtıcı
    /// - Observer Pattern
    /// - Oyun eventlerini dinleyicilere dağıtır
    /// - Loose coupling sağlar (oyun mantığı UI'dan bağımsız)
    /// </summary>
    public class GameEventDispatcher
    {
        private readonly List<IGameEventListener> _listeners;
        private readonly List<IGameAction> _eventHistory;

        public IReadOnlyList<IGameAction> EventHistory => _eventHistory.AsReadOnly();

        public GameEventDispatcher()
        {
            _listeners = new List<IGameEventListener>();
            _eventHistory = new List<IGameAction>();
        }

        /// <summary>
        /// Event dinleyici ekler
        /// </summary>
        public void Subscribe(IGameEventListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        /// <summary>
        /// Event dinleyici çıkarır
        /// </summary>
        public void Unsubscribe(IGameEventListener listener)
        {
            _listeners.Remove(listener);
        }

        /// <summary>
        /// Event yayınlar - tüm dinleyiciler bildirilir
        /// </summary>
        public void Dispatch(IGameAction action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            _eventHistory.Add(action);

            foreach (var listener in _listeners)
            {
                try
                {
                    listener.OnGameEvent(action);
                }
                catch (Exception ex)
                {
                    // Bir listener hata verse bile diğerlerine gönder
                    // Loglama yapılabilir
                    Console.WriteLine($"Event listener error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Belirli bir event tipine göre geçmişi filtreler
        /// </summary>
        public IReadOnlyList<IGameAction> GetEventsByType(GameActionType actionType)
        {
            return _eventHistory.Where(e => e.ActionType == actionType).ToList().AsReadOnly();
        }

        /// <summary>
        /// Event geçmişini temizler
        /// </summary>
        public void ClearHistory()
        {
            _eventHistory.Clear();
        }
    }

    /// <summary>
    /// Oyun durumu validator'ı
    /// - Single Responsibility: Sadece validasyon
    /// - Ortak validasyon mantığı
    /// </summary>
    public static class GameStateValidator
    {
        public static bool CanStartGame(IGameState? state, IReadOnlyList<IPlayer>? players, int minPlayers, int maxPlayers)  // DÜZELTME: nullable
        {
            if (state == null || players == null)
                return false;

            if (state.Status != GameStatus.Waiting)
                return false;

            var activePlayers = players.Count(p => 
                p.Status == PlayerStatus.Active || p.Status == PlayerStatus.Waiting);

            return activePlayers >= minPlayers && activePlayers <= maxPlayers;
        }

        public static bool IsGameInProgress(IGameState? state)  // DÜZELTME: nullable
        {
            return state?.Status == GameStatus.InProgress;
        }

        public static bool IsGameCompleted(IGameState? state)  // DÜZELTME: nullable
        {
            return state?.Status == GameStatus.Completed || state?.Status == GameStatus.Cancelled;
        }
    }
}