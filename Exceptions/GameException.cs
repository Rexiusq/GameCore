using System;

namespace GameCore.Exceptions
{
    /// <summary>
    /// Temel oyun exception sınıfı
    /// - Tüm oyun hataları buradan türer
    /// - Hata tipleri ayrıştırılabilir
    /// </summary>
    public abstract class GameException : Exception
    {
        public string? GameId { get; }  // DÜZELTME: nullable eklendi
        public DateTime OccurredAt { get; }

        protected GameException(string message, string? gameId = null)  // DÜZELTME: nullable
            : base(message)
        {
            GameId = gameId;
            OccurredAt = DateTime.UtcNow;
        }

        protected GameException(string message, Exception innerException, string? gameId = null)  // DÜZELTME: nullable
            : base(message, innerException)
        {
            GameId = gameId;
            OccurredAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Geçersiz oyun durumu hatası
    /// Örnek: Oyun başlamadan önce tur başlatmaya çalışmak
    /// </summary>
    public class InvalidGameStateException : GameException
    {
        public InvalidGameStateException(string message, string? gameId = null)  // DÜZELTME: nullable
            : base(message, gameId)
        {
        }
    }

    /// <summary>
    /// Geçersiz oyuncu aksiyonu hatası
    /// Örnek: Sırası olmayan oyuncunun aksiyon alması
    /// </summary>
    public class InvalidPlayerActionException : GameException
    {
        public string PlayerId { get; }

        public InvalidPlayerActionException(string message, string playerId, string? gameId = null)  // DÜZELTME: nullable
            : base(message, gameId)
        {
            PlayerId = playerId;
        }
    }

    /// <summary>
    /// Oyun kuralı ihlali hatası
    /// Örnek: UNO'da yanlış renk kart atmak
    /// </summary>
    public class GameRuleViolationException : GameException
    {
        public string RuleName { get; }

        public GameRuleViolationException(string message, string ruleName, string? gameId = null)  // DÜZELTME: nullable
            : base(message, gameId)
        {
            RuleName = ruleName;
            RuleName = ruleName;
        }
    }

    /// <summary>
    /// Oyuncu bulunamadı hatası
    /// </summary>
    public class PlayerNotFoundException : GameException
    {
        public string PlayerId { get; }

        public PlayerNotFoundException(string playerId, string? gameId = null)  // DÜZELTME: nullable
            : base($"Player {playerId} not found", gameId)
        {
            PlayerId = playerId;
        }
    }

    /// <summary>
    /// Oyun bulunamadı hatası
    /// </summary>
    public class GameNotFoundException : GameException
    {
        public GameNotFoundException(string gameId) 
            : base($"Game {gameId} not found", gameId)
        {
        }
    }

    /// <summary>
    /// Oyun kapasitesi aşıldı hatası
    /// Örnek: Maximum oyuncu sayısını aşmak
    /// </summary>
    public class GameCapacityExceededException : GameException
    {
        public int MaxCapacity { get; }
        public int CurrentCount { get; }

        public GameCapacityExceededException(int maxCapacity, int currentCount, string? gameId = null)  // DÜZELTME: nullable
            : base($"Game capacity exceeded. Max: {maxCapacity}, Current: {currentCount}", gameId)
        {
            MaxCapacity = maxCapacity;
            CurrentCount = currentCount;
        }
    }
}