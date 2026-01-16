using GameCore.Interfaces;
using GameCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameCore.Models
{
    /// <summary>
    /// Temel oyuncu sınıfı
    /// - IPlayer interface'ini implement eder
    /// - JSON serialization için hazır
    /// - Immutable properties kullanarak veri bütünlüğü sağlar
    /// </summary>
    public class Player : IPlayer
    {
        public string Id { get; }
        public string Name { get; }
        public PlayerStatus Status { get; set; }
        public DateTime JoinedAt { get; }

        /// <summary>
        /// Oyuncu skoru (isteğe bağlı, UNO gibi skorlu oyunlar için)
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Özel oyuncu verileri (her oyun kendi kullanabilir)
        /// Örnek: UNO'da elindeki kartlar, Vampir'de rolü vs.
        /// </summary>
        [JsonIgnore]
        public object? CustomData { get; set; }  

        public Player(string id, string name)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Player ID cannot be empty", nameof(id));
            
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Player name cannot be empty", nameof(name));

            Id = id;
            Name = name;
            Status = PlayerStatus.Waiting;
            JoinedAt = DateTime.UtcNow;
            Score = 0;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
    }

    /// <summary>
    /// Temel oyun state sınıfı
    /// - Abstract: Her oyun kendi state'ini türetecek
    /// - Template Method Pattern
    /// </summary>
    public abstract class BaseGameState : IGameState
    {
        public string GameId { get; protected set; }
        public GameStatus Status { get; set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>Oyunun hangi turda olduğu</summary>
        public int CurrentRound { get; set; }

        /// <summary>Toplam kaç tur oynanacak (opsiyonel)</summary>
        public int? MaxRounds { get; set; }

        protected BaseGameState(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException("Game ID cannot be empty", nameof(gameId));

            GameId = gameId;
            Status = GameStatus.Waiting;
            CreatedAt = DateTime.UtcNow;
            CurrentRound = 0;
        }

        public virtual string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }

        /// <summary>
        /// State'i güncellenmiş olarak işaretler
        /// </summary>
        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Tur bilgilerini tutan sınıf
    /// - Hangi oyuncunun sırası olduğunu takip eder
    /// - Tur geçmişi için kullanılabilir
    /// </summary>
    public class Turn
    {
        public int TurnNumber { get; }
        public string PlayerId { get; }
        public TurnStatus Status { get; set; }
        public DateTime StartedAt { get; }
        public DateTime? CompletedAt { get; set; }

        /// <summary>Bu turda yapılan aksiyonlar</summary>
        public string? ActionData { get; set; }  // DÜZELTME: nullable eklendi

        public Turn(int turnNumber, string playerId)
        {
            TurnNumber = turnNumber;
            PlayerId = playerId;
            Status = TurnStatus.Pending;
            StartedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            Status = TurnStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }

        public void Skip()
        {
            Status = TurnStatus.Skipped;
            CompletedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Temel oyun sınıfı
    /// - Abstract: Her oyun bu sınıftan türeyecek
    /// - Template Method Pattern kullanır
    /// - Ortak davranışları burada tanımlar
    /// </summary>
    public abstract class BaseGame : IGame
    {
        private readonly List<IPlayer> _players;

        public string GameId { get; }
        public IGameState State { get; protected set; }
        public IReadOnlyList<IPlayer> Players => _players.AsReadOnly();
        
        protected IGameRules Rules { get; }
        protected ITurnManager? TurnManager { get; set; }  // DÜZELTME: nullable eklendi

        protected BaseGame(string gameId, IGameRules rules)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException("Game ID cannot be empty", nameof(gameId));

            GameId = gameId;
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _players = new List<IPlayer>();
            
            // DÜZELTME: State başlangıçta null olmamalı, bu yüzden alt sınıflar initialize etmeli
            State = null!; // Alt sınıflar constructor'da set edecek
        }

        public virtual void AddPlayer(IPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (_players.Count >= Rules.MaxPlayers)
                throw new InvalidOperationException($"Maximum player limit ({Rules.MaxPlayers}) reached");

            if (_players.Any(p => p.Id == player.Id))
                throw new InvalidOperationException($"Player {player.Id} already exists");

            _players.Add(player);
        }

        public virtual void RemovePlayer(string playerId)
        {
            var player = _players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.Status = PlayerStatus.Disconnected;
                _players.Remove(player);
            }
        }

        public virtual void StartGame()
        {
            if (!Rules.CanStartGame(Players))
                throw new InvalidOperationException("Cannot start game: insufficient players or other rule violation");

            State.Status = GameStatus.InProgress;
            OnGameStarted();
        }

        public virtual void EndGame()
        {
            State.Status = GameStatus.Completed;
            OnGameEnded();
        }

        public string GetStateJson()
        {
            return State.ToJson();
        }

        // Template methods - alt sınıflar override edebilir
        protected abstract void OnGameStarted();
        protected abstract void OnGameEnded();
    }


    /// Temel oyun aksiyonu sınıfı
    /// - Command Pattern
    /// - Her aksiyon validate + execute edilebilir
    public abstract class BaseGameAction : IGameAction
    {
        public string ActionId { get; }
        public GameActionType ActionType { get; }
        public string PlayerId { get; }
        public DateTime Timestamp { get; }

        protected BaseGameAction(GameActionType actionType, string playerId)
        {
            ActionId = Guid.NewGuid().ToString();
            ActionType = actionType;
            PlayerId = playerId;
            Timestamp = DateTime.UtcNow;
        }

        public abstract bool Validate(IGameState state);
        public abstract void Execute(IGameState state);

        public virtual string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
    }
}
