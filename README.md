# 🎮 GameCore Kütüphanesi - Geliştirici Dokümantasyonu

> **Versiyon:** 1.0  
> **Tarih:** Ocak 2025  
> **Platform:** .NET 9.0  
> **Amaç:** Tur tabanlı multiplayer oyunlar için ortak bir temel kütüphane

---

## 📚 İçindekiler

1. [GameCore Nedir?](#gamecore-nedir)
2. [Hızlı Başlangıç](#hızlı-başlangıç)
3. [Mimari Yapı](#mimari-yapı)
4. [Temel Kavramlar](#temel-kavramlar)
5. [Nasıl Kullanılır - Adım Adım](#nasıl-kullanılır)
6. [Sınıf Referansları](#sınıf-referansları)
7. [En İyi Uygulamalar](#en-iyi-uygulamalar)
8. [Sık Yapılan Hatalar](#sık-yapılan-hatalar)
9. [Örnek Oyunlar](#örnek-oyunlar)

---

## 🎯 GameCore Nedir?

**GameCore**, tur tabanlı multiplayer oyunlar (UNO, Taboo, Vampir Köylü vb.) için **ortak bir temel** sağlayan bir C# kütüphanesidir.

### Neden GameCore?

❌ **GameCore Olmadan:**
```
Her oyunda ayrı ayrı yazman gerekenler:
- Player sınıfı
- Turn (Tur) yönetimi
- Game state yönetimi
- Event sistemi
- JSON serialization
- Oyuncu durumları
... ve daha fazlası
```

✅ **GameCore İle:**
```csharp
// Sadece oyunun kendine özgü mantığını yaz!
public class MyGame : BaseGame { ... }
public class MyGameState : BaseGameState { ... }
```

### Temel Prensipler

GameCore şu SOLID prensiplerine göre tasarlanmıştır:

- **Single Responsibility:** Her sınıf tek bir işten sorumlu
- **Open/Closed:** Genişlemeye açık, değişime kapalı
- **Liskov Substitution:** Alt sınıflar birbirinin yerine kullanılabilir
- **Interface Segregation:** Küçük, odaklanmış interface'ler
- **Dependency Inversion:** Abstraction'a bağımlılık

---

## ⚡ Hızlı Başlangıç

### Adım 1: GameCore'u Projeye Ekle

```bash
# Yeni oyun projesi oluştur
dotnet new console -n MyGame

# GameCore referansını ekle
cd MyGame
dotnet add reference ../GameCore/GameCore.csproj
```

### Adım 2: İlk Oyuncunu Oluştur

```csharp
using GameCore.Models;

var player = new Player("p1", "Ahmet");
Console.WriteLine($"Oyuncu: {player.Name}");
```

### Adım 3: Oyununuzu Yazın

```csharp
using GameCore.Models;
using GameCore.Interfaces;

public class MyGameState : BaseGameState
{
    public MyGameState(string gameId) : base(gameId) { }
}

public class MyGame : BaseGame
{
    public MyGame(string gameId, IGameRules rules) 
        : base(gameId, rules) 
    {
        State = new MyGameState(gameId);
    }

    protected override void OnGameStarted() 
    { 
        // Oyun başladığında yapılacaklar
    }

    protected override void OnGameEnded() 
    { 
        // Oyun bittiğinde yapılacaklar
    }
}
```

---

## 🏗️ Mimari Yapı

GameCore, **katmanlı mimari** kullanır:

```
┌─────────────────────────────────────┐
│         OYUN KATMANI                │
│  (UnoGame, TabooGame, vs.)          │
├─────────────────────────────────────┤
│       GAMECORE KÜTÜPHANESİ          │
│  ┌───────────┬──────────┬────────┐  │
│  │ Models    │ Managers │ Events │  │
│  ├───────────┼──────────┼────────┤  │
│  │ Interfaces│  Enums   │Except. │  │
│  └───────────┴──────────┴────────┘  │
├─────────────────────────────────────┤
│          .NET FRAMEWORK             │
└─────────────────────────────────────┘
```

### Klasör Yapısı

```
GameCore/
├── Enums/                    # Sabit değerler
│   └── BasicConstants.cs     # GameStatus, PlayerStatus, vb.
│
├── Interfaces/               # Sözleşmeler
│   └── Interface.cs          # IPlayer, IGame, IGameState, vb.
│
├── Models/                   # Temel veri modelleri
│   └── BasicDataModels.cs    # Player, BaseGame, BaseGameState
│
├── Managers/                 # İş mantığı yöneticileri
│   └── GameManagers.cs       # TurnManager, PlayerManager
│
└── Exceptions/               # Özel hatalar
    └── GameException.cs      # InvalidGameStateException, vb.
```

---

## 💡 Temel Kavramlar

### 1. Player (Oyuncu)

Her oyuncu bir **IPlayer** interface'ini implement eder.

```csharp
public interface IPlayer
{
    string Id { get; }           // Benzersiz ID
    string Name { get; }         // Oyuncu adı
    PlayerStatus Status { get; } // Aktif, Bekleniyor, Elendi vb.
    DateTime JoinedAt { get; }   // Katılma zamanı
}
```

**Hazır Kullanım:**
```csharp
var player = new Player("p1", "Ahmet");
// ID: "p1"
// Name: "Ahmet"  
// Status: PlayerStatus.Waiting
// JoinedAt: DateTime.UtcNow
```

---

### 2. Game State (Oyun Durumu)

Oyunun **anlık durumunu** temsil eder. JSON olarak serialize edilebilir.

```csharp
public interface IGameState
{
    string GameId { get; }
    GameStatus Status { get; }      // Waiting, InProgress, Completed
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
    string ToJson();                // Backend'e göndermek için
}
```

**Nasıl Kullanılır?**
```csharp
// Kendi oyunun için extend et
public class UnoGameState : BaseGameState
{
    public UnoCard LastPlayedCard { get; set; }  // UNO'ya özel
    
    public UnoGameState(string gameId) : base(gameId) { }
}
```

---

### 3. Game (Oyun)

Ana oyun sınıfı. **BaseGame**'den türetilir.

```csharp
public interface IGame
{
    string GameId { get; }
    IGameState State { get; }
    IReadOnlyList<IPlayer> Players { get; }
    
    void StartGame();
    void EndGame();
    void AddPlayer(IPlayer player);
    void RemovePlayer(string playerId);
}
```

**Template Method Pattern:**
```csharp
public class MyGame : BaseGame
{
    public MyGame(string gameId) : base(gameId, new MyGameRules())
    {
        State = new MyGameState(gameId);
    }

    // Alt sınıflar bu metodları override eder
    protected override void OnGameStarted() 
    { 
        // Kartları dağıt, ilk turu başlat vb.
    }

    protected override void OnGameEnded() 
    { 
        // Kazananı belirle, skorları kaydet vb.
    }
}
```

---

### 4. Turn Manager (Tur Yöneticisi)

Sıra tabanlı oyunlarda **kiminle sırada** olduğunu takip eder.

```csharp
public interface ITurnManager
{
    IPlayer CurrentPlayer { get; }      // Şu an kimin sırası?
    int CurrentTurnNumber { get; }      // Kaçıncı tur?
    TurnStatus TurnStatus { get; }      // Aktif, Bekleniyor vb.
    
    void StartTurn();                   // Turu başlat
    void EndTurn();                     // Turu bitir
    void NextTurn();                    // Sonraki oyuncuya geç
    bool IsPlayerTurn(string playerId); // Bu oyuncunun sırası mı?
}
```

**Kullanım:**
```csharp
// Oyun başlarken oluştur
TurnManager = new TurnManager(Players);
TurnManager.StartTurn();

// Bir aksiyon alındığında
if (!TurnManager.IsPlayerTurn(playerId))
    throw new Exception("Senin sıran değil!");

// Tur bitti, sonrakine geç
TurnManager.EndTurn();
TurnManager.NextTurn();
```

**Özellikler:**
- ✅ Circular iteration (son oyuncudan sonra ilke döner)
- ✅ Elenen oyuncuları otomatik atlar
- ✅ Tur geçmişi tutar (`TurnHistory`)

---

### 5. Game Rules (Oyun Kuralları)

Her oyunun **kendi kurallarını** tanımlar.

```csharp
public interface IGameRules
{
    int MinPlayers { get; }             // Minimum oyuncu sayısı
    int MaxPlayers { get; }             // Maximum oyuncu sayısı
    
    bool CanStartGame(...);             // Oyun başlayabilir mi?
    bool ValidateAction(...);           // Aksiyon geçerli mi?
    bool IsGameOver(...);               // Oyun bitti mi?
    IPlayer? GetWinner(...);            // Kazanan kim?
}
```

**Örnek:**
```csharp
public class UnoGameRules : IGameRules
{
    public int MinPlayers => 2;
    public int MaxPlayers => 10;
    
    public bool CanStartGame(IReadOnlyList<IPlayer> players)
    {
        return players.Count >= MinPlayers && players.Count <= MaxPlayers;
    }
    
    // ... diğer metodlar
}
```

---

### 6. Game Actions (Oyun Aksiyonları)

**Command Pattern** kullanır. Her aksiyon:
- Validate edilir (geçerli mi?)
- Execute edilir (çalıştır)
- JSON'a serialize edilir (log için)

```csharp
public interface IGameAction
{
    string ActionId { get; }
    GameActionType ActionType { get; }
    string PlayerId { get; }
    DateTime Timestamp { get; }
    
    bool Validate(IGameState state);
    void Execute(IGameState state);
    string ToJson();
}
```

**Örnek:**
```csharp
public class PlayCardAction : BaseGameAction
{
    public Card Card { get; }
    
    public PlayCardAction(string playerId, Card card) 
        : base(GameActionType.PlayerAction, playerId)
    {
        Card = card;
    }
    
    public override bool Validate(IGameState state)
    {
        // Kartın atılabilir olup olmadığını kontrol et
        return CanPlayCard(Card);
    }
    
    public override void Execute(IGameState state)
    {
        // State'i güncelle
        state.LastPlayedCard = Card;
        state.MarkAsUpdated();
    }
}
```

---

## 📖 Nasıl Kullanılır - Adım Adım

### Senaryo: UNO Benzeri Bir Oyun Yapalım

#### Adım 1: Game State Oluştur

```csharp
using GameCore.Models;

public class MyCardGameState : BaseGameState
{
    // Oyuna özel state bilgileri
    public Card? LastPlayedCard { get; set; }
    public bool IsClockwise { get; set; }
    public int DrawPenalty { get; set; }
    
    public MyCardGameState(string gameId) : base(gameId)
    {
        IsClockwise = true;
        DrawPenalty = 0;
    }
}
```

**Neden BaseGameState?**
- ✅ `GameId`, `Status`, `CreatedAt` otomatik gelir
- ✅ `ToJson()` metodu hazır
- ✅ `MarkAsUpdated()` ile timestamp otomatik

---

#### Adım 2: Oyun Kurallarını Tanımla

```csharp
using GameCore.Interfaces;

public class MyCardGameRules : IGameRules
{
    public int MinPlayers => 2;
    public int MaxPlayers => 10;
    
    public bool CanStartGame(IReadOnlyList<IPlayer> players)
    {
        var activeCount = players.Count(p => 
            p.Status == PlayerStatus.Active || 
            p.Status == PlayerStatus.Waiting);
        
        return activeCount >= MinPlayers && activeCount <= MaxPlayers;
    }
    
    public bool ValidateAction(IGameAction action, IGameState state)
    {
        // Aksiyonun geçerli olup olmadığını kontrol et
        if (action is PlayCardAction playAction)
        {
            return CanPlayCard(playAction.Card);
        }
        return true;
    }
    
    public bool IsGameOver(IGameState state)
    {
        // Oyun bitişi kontrolü
        return false; // Implementasyon gerekli
    }
    
    public IPlayer? GetWinner(IGameState state)
    {
        // Kazananı belirle
        return null; // Implementasyon gerekli
    }
}
```

---

#### Adım 3: Ana Oyun Sınıfını Oluştur

```csharp
using GameCore.Models;
using GameCore.Managers;

public class MyCardGame : BaseGame
{
    private readonly MyCardGameState _gameState;
    
    public MyCardGame(string gameId) 
        : base(gameId, new MyCardGameRules())
    {
        _gameState = new MyCardGameState(gameId);
        State = _gameState;
    }
    
    protected override void OnGameStarted()
    {
        // 1. Kartları dağıt
        DistributeCards();
        
        // 2. İlk kartı ortaya koy
        _gameState.LastPlayedCard = DrawInitialCard();
        
        // 3. Tur sistemini başlat
        TurnManager = new TurnManager(Players);
        TurnManager.StartTurn();
        
        Console.WriteLine("Oyun başladı!");
    }
    
    protected override void OnGameEnded()
    {
        // Oyun sonu işlemleri
        var winner = Rules.GetWinner(State);
        Console.WriteLine($"Oyun bitti! Kazanan: {winner?.Name}");
    }
    
    // Oyuna özel public metodlar
    public void PlayCard(string playerId, Card card)
    {
        // 1. Sıra kontrolü
        if (!TurnManager!.IsPlayerTurn(playerId))
            throw new InvalidPlayerActionException(
                "Senin sıran değil!", playerId, GameId);
        
        // 2. Aksiyon oluştur ve validate et
        var action = new PlayCardAction(playerId, card);
        if (!action.Validate(State))
            throw new GameRuleViolationException(
                "Geçersiz kart!", "CardValidation", GameId);
        
        // 3. Aksiyonu çalıştır
        action.Execute(State);
        
        // 4. Turu ilerlet
        TurnManager.EndTurn();
        TurnManager.NextTurn();
    }
    
    private void DistributeCards() { /* ... */ }
    private Card DrawInitialCard() { return new Card(); }
}
```

---

#### Adım 4: Oyunu Kullan

```csharp
// Program.cs

var game = new MyCardGame("game-123");

// Oyuncular ekle
game.AddPlayer(new Player("p1", "Ahmet"));
game.AddPlayer(new Player("p2", "Mehmet"));
game.AddPlayer(new Player("p3", "Ayşe"));

// Oyunu başlat
game.StartGame();

// Kart at
var card = new Card { Color = CardColor.Red, Number = 5 };
game.PlayCard("p1", card);

// State'i JSON olarak al (Backend'e göndermek için)
string stateJson = game.GetStateJson();
Console.WriteLine(stateJson);
```

---

## 📚 Sınıf Referansları

### Models (Modeller)

#### Player
```csharp
public class Player : IPlayer
{
    public string Id { get; }              // Oyuncu ID
    public string Name { get; }            // İsim
    public PlayerStatus Status { get; set; } // Durum
    public DateTime JoinedAt { get; }      // Katılma zamanı
    public int Score { get; set; }         // Skor
    public object? CustomData { get; set; } // Özel veri
}
```

**Kullanım:**
```csharp
var player = new Player("p1", "Ahmet");
player.Score = 100;
player.Status = PlayerStatus.Active;
```

---

#### BaseGameState
```csharp
public abstract class BaseGameState : IGameState
{
    public string GameId { get; }
    public GameStatus Status { get; set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; set; }
    public int CurrentRound { get; set; }
    public int? MaxRounds { get; set; }
    
    public void MarkAsUpdated();
    public virtual string ToJson();
}
```

**Nasıl Extend Edilir:**
```csharp
public class MyState : BaseGameState
{
    public string MyCustomProperty { get; set; }
    
    public MyState(string gameId) : base(gameId) { }
}
```

---

#### BaseGame
```csharp
public abstract class BaseGame : IGame
{
    public string GameId { get; }
    public IGameState State { get; }
    public IReadOnlyList<IPlayer> Players { get; }
    protected IGameRules Rules { get; }
    protected ITurnManager? TurnManager { get; set; }
    
    public virtual void StartGame();
    public virtual void EndGame();
    public virtual void AddPlayer(IPlayer player);
    public virtual void RemovePlayer(string playerId);
    public string GetStateJson();
    
    // Override edilmesi gereken metodlar
    protected abstract void OnGameStarted();
    protected abstract void OnGameEnded();
}
```

---

### Managers (Yöneticiler)

#### TurnManager

**Constructor:**
```csharp
var turnManager = new TurnManager(players);
```

**Metodlar:**
```csharp
turnManager.StartTurn();              // Turu başlat
turnManager.EndTurn();                // Turu bitir
turnManager.NextTurn();               // Sonraki oyuncuya geç
bool isMyTurn = turnManager.IsPlayerTurn("p1");
turnManager.EliminatePlayer("p2");    // Oyuncuyu elen
turnManager.ShuffleOrder();           // Sırayı karıştır
```

**Properties:**
```csharp
IPlayer current = turnManager.CurrentPlayer;
int turnNo = turnManager.CurrentTurnNumber;
TurnStatus status = turnManager.TurnStatus;
var history = turnManager.TurnHistory;
```

---

#### PlayerManager

```csharp
var playerManager = new PlayerManager();

playerManager.AddPlayer(player);
playerManager.RemovePlayer("p1");
IPlayer? player = playerManager.GetPlayer("p1");
bool exists = playerManager.HasPlayer("p1");
playerManager.UpdatePlayerStatus("p1", PlayerStatus.Active);
playerManager.SetAllPlayersStatus(PlayerStatus.Active);

// Properties
int total = playerManager.PlayerCount;
int active = playerManager.ActivePlayerCount;
var all = playerManager.AllPlayers;
var actives = playerManager.ActivePlayers;
```

---

#### GameEventDispatcher

```csharp
var dispatcher = new GameEventDispatcher();

// Listener ekle
dispatcher.Subscribe(myListener);

// Event gönder
dispatcher.Dispatch(action);

// Geçmişi görüntüle
var allEvents = dispatcher.EventHistory;
var playerActions = dispatcher.GetEventsByType(GameActionType.PlayerAction);

// Temizle
dispatcher.ClearHistory();
```

---

### Enums (Sabitler)

```csharp
// Oyun durumu
public enum GameStatus
{
    Waiting,      // Oyuncular bekleniyor
    InProgress,   // Oyun devam ediyor
    Paused,       // Duraklatıldı
    Completed,    // Tamamlandı
    Cancelled     // İptal edildi
}

// Oyuncu durumu
public enum PlayerStatus
{
    Active,       // Aktif
    Waiting,      // Bekliyor
    Disconnected, // Bağlantı kesildi
    Eliminated,   // Elendi
    Winner        // Kazandı
}

// Tur durumu
public enum TurnStatus
{
    Pending,      // Henüz başlamadı
    Active,       // Aktif
    Completed,    // Tamamlandı
    Skipped       // Atlandı
}

// Aksiyon tipleri
public enum GameActionType
{
    GameStarted,
    GameEnded,
    TurnStarted,
    TurnEnded,
    PlayerAction,
    PlayerJoined,
    PlayerLeft,
    CustomEvent = 999
}
```

---

### Exceptions (Hatalar)

```csharp
// Geçersiz oyun durumu
throw new InvalidGameStateException(
    "Oyun henüz başlamadı", gameId);

// Geçersiz oyuncu aksiyonu
throw new InvalidPlayerActionException(
    "Senin sıran değil", playerId, gameId);

// Kural ihlali
throw new GameRuleViolationException(
    "Geçersiz kart", "CardRule", gameId);

// Oyuncu bulunamadı
throw new PlayerNotFoundException(playerId, gameId);

// Oyun bulunamadı
throw new GameNotFoundException(gameId);

// Kapasite aşıldı
throw new GameCapacityExceededException(
    maxPlayers: 10, currentCount: 11, gameId);
```

---

## ✅ En İyi Uygulamalar

### 1. State'i Düzenli Güncelle

```csharp
// ❌ YANLIŞ
_gameState.LastPlayedCard = card;

// ✅ DOĞRU
_gameState.LastPlayedCard = card;
_gameState.MarkAsUpdated(); // Timestamp'i günceller
```

---

### 2. Her Zaman Validate Et

```csharp
// ❌ YANLIŞ
public void PlayCard(string playerId, Card card)
{
    _gameState.LastPlayedCard = card;
}

// ✅ DOĞRU
public void PlayCard(string playerId, Card card)
{
    if (!TurnManager.IsPlayerTurn(playerId))
        throw new InvalidPlayerActionException(...);
        
    var action = new PlayCardAction(playerId, card);
    if (!action.Validate(State))
        throw new GameRuleViolationException(...);
        
    action.Execute(State);
}
```

---

### 3. Null Kontrolleri Yap

```csharp
// ✅ DOĞRU
if (TurnManager == null)
    throw new InvalidGameStateException("Oyun başlatılmamış");

if (!TurnManager.IsPlayerTurn(playerId))
    throw new InvalidPlayerActionException(...);
```

---

### 4. JSON Serialization İçin Hazırlan

```csharp
// State'iniz backend'e gönderilecek, bu yüzden:

// ✅ Basit tipler kullanın
public int Score { get; set; }
public string Name { get; set; }

// ✅ Nullable yapın gerekirse
public Card? LastCard { get; set; }

// ❌ Circular reference'tan kaçının
public Player Player { get; set; }
public Game Game { get; set; } // Player -> Game -> Player -> ...
```

---

### 5. CustomData'yı Akıllıca Kullan

```csharp
// Player için özel veri
var player = new Player("p1", "Ahmet");

// UNO'da elindeki kartlar
player.CustomData = new List<Card> { card1, card2 };

// Vampir'de rolü
player.CustomData = new { Role = "Vampir", IsRevealed = false };
```

---

## ⚠️ Sık Yapılan Hatalar

### Hata 1: State'i Constructor'da Set Etmemek

```csharp
// ❌ YANLIŞ
public class MyGame : BaseGame
{
    public MyGame(string gameId) : base(gameId, rules)
    {
        // State set edilmedi!
    }
}

// ✅ DOĞRU
public class MyGame : BaseGame
{
    public MyGame(string gameId) : base(gameId, rules)
    {
        State = new MyGameState(gameId); // ✅
    }
}
```

---

### Hata 2: TurnManager'ı OnGameStarted'da Oluşturmamak

```csharp
// ❌ YANLIŞ
public class MyGame : BaseGame
{
    public MyGame(string gameId) : base(gameId, rules)
    {
        TurnManager = new TurnManager(Players); // Henüz oyuncu yok!
    }
}

// ✅ DOĞRU
protected override void OnGameStarted()
{
    TurnManager = new TurnManager(Players); // Oyuncular eklendikten sonra
    TurnManager.StartTurn();
}
```

---

### Hata 3: Sıra Kontrolü Yapmadan Aksiyon Almak

```csharp
// ❌ YANLIŞ
public void DoSomething(string playerId)
{
    // Direkt yap
    _gameState.SomeProperty = value;
}

// ✅ DOĞRU
public void DoSomething(string playerId)
{
    if (!TurnManager!.IsPlayerTurn(playerId))
        throw new InvalidPlayerActionException("Not your turn", playerId);
        
    _gameState.SomeProperty = value;
}
```

---

### Hata 4: Interface'leri Concrete Class'larla Karıştırmak

```csharp
// ❌ YANLIŞ
IPlayer player = new IPlayer(); // Interface new'lenemez!

// ✅ DOĞRU
IPlayer player = new Player("p1", "Ahmet");
```

---

## 🎮 Örnek Oyunlar

### Örnek 1: Basit Zar Oyunu

```csharp
public class DiceGameState : BaseGameState
{
    public int LastRoll { get; set; }
    public Dictionary<string, int> Scores { get; set; } = new();
    
    public DiceGameState(string gameId) : base(gameId) { }
}

public class DiceGameRules : IGameRules
{
    public int MinPlayers => 2;
    public int MaxPlayers => 6;
    
    public bool CanStartGame(IReadOnlyList<IPlayer> players)
        => players.Count >= MinPlayers;
    
    public bool ValidateAction(IGameAction action, IGameState state) => true;
    
    public bool IsGameOver(IGameState state)
    {
        var diceState = state as DiceGameState;
        return diceState?.Scores.Any(s => s.Value >= 100) ?? false;
    }
    
    public IPlayer? GetWinner(IGameState state)
    {
        var diceState = state as DiceGameState;
        var winnerEntry = diceState?.Scores.MaxBy(s => s.Value);
        return null; // Gerçek implementasyonda player döndür
    }
}

public class DiceGame : BaseGame
{
    private readonly DiceGameState _state;
    private readonly Random _random = new();
    
    public DiceGame(string gameId) : base(gameId, new DiceGameRules())
    {
        _state = new DiceGameState(gameId);
        State = _state;
    }
    
    protected override void OnGameStarted()
    {
        TurnManager = new TurnManager(Players);
        TurnManager.StartTurn();
        
        foreach (var player in Players)
            _state.Scores[player.Id] = 0;
    }
    
    protected override void OnGameEnded() { }
    
    public void RollDice(string playerId)
    {
        if (!TurnManager!.IsPlayerTurn(playerId))
            throw new InvalidPlayerActionException("Not your turn", playerId);
        
        int roll = _random.Next(1, 7);
        _state.LastRoll = roll;
        _state.Scores[playerId] += roll;
        _state.MarkAsUpdated();
        
        if (Rules.IsGameOver(State))
            EndGame();
        else
        {
            TurnManager.EndTurn();
            TurnManager.NextTurn();
        }
    }
}

// Kullanım
var game = new DiceGame("dice-1");
game.AddPlayer(new Player("p1", "Ali"));
game.AddPlayer(new Player("p2", "Veli"));
game.StartGame();

game.RollDice("p1"); // Ali zar atıyor
game.RollDice("p2"); // Veli zar atıyor
```

---

## 🔗 Backend Entegrasyonu

### JSON State Gönderme

```csharp
// Oyun state'ini JSON'a çevir
string stateJson = game.GetStateJson();

// Backend'e POST et
var response = await httpClient.PostAsync(
    "api/games/state", 
    new StringContent(stateJson, Encoding.UTF8, "application/json")
);

// Database'e kaydet
await database.SaveGameState(gameId, stateJson);
```

### SignalR ile Broadcast

```csharp
// Controller'da
[HttpPost("games/{gameId}/action")]
public async Task<IActionResult> PlayAction(string gameId, ActionDto dto)
{
    var game = _gameRepository.GetGame(gameId);
    game.PlayCard(dto.PlayerId, dto.Card);
    
    // Tüm oyunculara broadcast et
    await _hubContext.Clients
        .Group(gameId)
        .SendAsync("GameUpdated", game.GetStateJson());
    
    return Ok();
}
```

---

## 📞 Destek ve Katkı

**Sorularınız mı var?**
- Takım lideri ile iletişime geçin
- Kod review sürecini takip edin
- Unit testlerinizi yazmayı unutmayın

**Katkıda bulunmak ister misiniz?**
1. Feature branch oluşturun
2. Kodunuzu yazın
3. Pull request açın
4. Code review bekleyin

---

## 📝 Versiyon Geçmişi

**v1.0 - Ocak 2025**
- ✅ İlk stabil sürüm
- ✅ Temel sınıflar (Player, BaseGame, BaseGameState)
- ✅ Tur yönetimi (TurnManager)
- ✅ Event sistemi (GameEventDispatcher)
- ✅ Exception handling
- ✅ JSON serialization desteği

---

## ⚡ Sonuç

GameCore ile:
- ✅ Daha az kod yazarsınız
- ✅ Tutarlı mimari kullanırsınız
- ✅ Kodunuz test edilebilir olur
- ✅ Backend entegrasyonu kolay olur
- ✅ Yeni oyunlar hızlı geliştirilir

**Mutlu kodlamalar! 🎮**