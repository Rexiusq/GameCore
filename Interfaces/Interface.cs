using GameCore.Enums;
using System;
using System.Collections.Generic;

namespace GameCore.Interfaces
{
    /// <summary>
    /// Oyuncu için temel sözleşme
    /// - Her oyuncunun olması gereken minimum özellikler
    /// - JSON serialization için kullanılabilir
    /// </summary>
    public interface IPlayer
    {
        string Id { get; }
        string Name { get; }
        PlayerStatus Status { get; set; }
        DateTime JoinedAt { get; }
    }

    /// <summary>
    /// Oyun state'i için temel sözleşme
    /// - Her oyunun state'ini serialize edebilmek için
    /// - Database'e kaydetmek için gerekli
    /// </summary>
    public interface IGameState
    {
        string GameId { get; }
        GameStatus Status { get; set; }  // DÜZELTME: set eklendi
        DateTime CreatedAt { get; }
        DateTime? UpdatedAt { get; set; }  // DÜZELTME: set eklendi
        
        /// <summary>JSON serialization için</summary>
        string ToJson();
    }

    /// <summary>
    /// Oyun için temel sözleşme
    /// - Tüm oyunların implement etmesi gereken temel metodlar
    /// - Open/Closed Principle: Genişlemeye açık, değişime kapalı
    /// </summary>
    public interface IGame
    {
        string GameId { get; }
        IGameState State { get; }
        IReadOnlyList<IPlayer> Players { get; }
        
        /// <summary>Oyunu başlatır</summary>
        void StartGame();
        
        /// <summary>Oyunu bitirir</summary>
        void EndGame();
        
        /// <summary>Oyuncu ekler</summary>
        void AddPlayer(IPlayer player);
        
        /// <summary>Oyuncu çıkarır</summary>
        void RemovePlayer(string playerId);
        
        /// <summary>Mevcut durumu JSON olarak döndürür</summary>
        string GetStateJson();
    }

    /// <summary>
    /// Tur yönetimi için sözleşme
    /// - Sıra tabanlı oyunlar için kritik
    /// - Interface Segregation: Sadece tur ile ilgili metodlar
    /// </summary>
    public interface ITurnManager
    {
        IPlayer CurrentPlayer { get; }
        int CurrentTurnNumber { get; }
        TurnStatus TurnStatus { get; }
        
        /// <summary>Bir sonraki oyuncuya geçer</summary>
        void NextTurn();
        
        /// <summary>Turu başlatır</summary>
        void StartTurn();
        
        /// <summary>Turu bitirir</summary>
        void EndTurn();
        
        /// <summary>Belirli bir oyuncunun sırası mı kontrol eder</summary>
        bool IsPlayerTurn(string playerId);
    }

    /// <summary>
    /// Oyun aksiyonları için sözleşme
    /// - Command Pattern benzeri
    /// - Her aksiyon validate edilebilir ve execute edilebilir
    /// </summary>
    public interface IGameAction
    {
        string ActionId { get; }
        GameActionType ActionType { get; }
        string PlayerId { get; }
        DateTime Timestamp { get; }
        
        /// <summary>Aksiyonun geçerli olup olmadığını kontrol eder</summary>
        bool Validate(IGameState state);
        
        /// <summary>Aksiyonu çalıştırır</summary>
        void Execute(IGameState state);
        
        /// <summary>JSON olarak serialize eder</summary>
        string ToJson();
    }

    /// <summary>
    /// Oyun kuralları için sözleşme
    /// - Strategy Pattern: Her oyun kendi kurallarını implement eder
    /// - Single Responsibility: Sadece kural kontrolü
    /// </summary>
    public interface IGameRules
    {
        /// <summary>Minimum oyuncu sayısı</summary>
        int MinPlayers { get; }
        
        /// <summary>Maximum oyuncu sayısı</summary>
        int MaxPlayers { get; }
        
        /// <summary>Oyunun başlayıp başlayamayacağını kontrol eder</summary>
        bool CanStartGame(IReadOnlyList<IPlayer> players);
        
        /// <summary>Aksiyonun kurallara uygun olup olmadığını kontrol eder</summary>
        bool ValidateAction(IGameAction action, IGameState state);
        
        /// <summary>Oyunun bitip bitmediğini kontrol eder</summary>
        bool IsGameOver(IGameState state);
        
        /// <summary>Kazananı belirler (varsa)</summary>
        IPlayer? GetWinner(IGameState state);  // DÜZELTME: nullable eklendi
    }

    /// <summary>
    /// Event dinleyicileri için sözleşme
    /// - Observer Pattern
    /// - Oyun olaylarını yakalamak için (UI güncelleme, logging vs.)
    /// </summary>
    public interface IGameEventListener
    {
        void OnGameEvent(IGameAction action);
    }
}