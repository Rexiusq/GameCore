
namespace GameCore.Enums
{
    
    /// Oyunun genel durumunu temsil eder
    public enum GameStatus
    {
        /// Oyun henüz başlamadı, oyuncular bekleniyor
        Waiting = 0,
        
        /// Oyun aktif olarak oynanıyor
        InProgress = 1,
        
        /// Oyun duraklatıldı (opsiyonel)
        Paused = 2,
        
        /// Oyun tamamlandı, kazanan belli oldu
        Completed = 3,
        
        /// Oyun iptal edildi
        Cancelled = 4
    }


    /// Oyuncunun oyundaki durumunu temsil eder
    public enum PlayerStatus
    {
        /// Oyuncu aktif ve oynuyor
        Active = 0,
        
        /// Oyuncu sırası bekleniyor
        Waiting = 1,
        
        /// Oyuncu oyunu terk etti
        Disconnected = 2,
        
        /// Oyuncu elendi (Vampir Köylü gibi oyunlar için)
        Eliminated = 3,
        
        /// Oyuncu kazandı
        Winner = 4
    }

    
    /// Tur durumunu temsil eder
    public enum TurnStatus
    {
        /// Tur henüz başlamadı
        Pending = 0,
        
        /// Tur aktif, oyuncu aksiyon alabilir
        Active = 1,
        
        /// Tur tamamlandı
        Completed = 2,
        
        /// Tur atlandı (timeout veya skip)
        Skipped = 3
    }

    /// Oyun aksiyonlarının tipi
    public enum GameActionType
    {
        /// Oyun başlatıldı
        GameStarted = 0,
        
        /// Oyun bitti<
        GameEnded = 1,
        
        /// Tur başladı
        TurnStarted = 2,
        
        /// Tur bitti
        TurnEnded = 3,
        
        /// Oyuncu aksiyon aldı (kart attı, kelime söyledi vs.)
        PlayerAction = 4,
        
        /// Oyuncu oyuna katıldı
        PlayerJoined = 5,
        
        /// Oyuncu oyundan ayrıldı
        PlayerLeft = 6,
        
        /// Özel oyun eventi (her oyun kendi tanımlayabilir)
        CustomEvent = 999
    }
}