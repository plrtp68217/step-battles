# ЗАПИСКА ПЛОТНЯЧКА - Mirror (высокоуровневая сетевая библиотека для Unity)

## Вместо MonoBehaviour используется NetworkBehaviour
## В скрипте может быть описана логика как для клиента, так и для сервера
### Для этого используются следующие маркеры методов
 - [Command]  -  метод выполняется на сервере                       (Название метода начинается с Cmd)
 - [ClientRpc]    -  метод выполняется на всех клиентах             (Название метода начинается с Rpc)
 - [TargetRpc]   -  метод выполняется на конкретном клиенте  (Название метода начинается с Target)

## Как работает сетевая логика
### Когда клиент подключается к серверу, он получает **копии всех сетевых объектов** (игроков, NPC и т.д.).

У ТЕБЯ на компьютере:            У Игрока2 на компьютере:        У Игрока3 на компьютере:
┌──────────────────────┐        ┌──────────────────────┐        ┌──────────────────────┐
│ PlayerScript(Ты)     │        │ PlayerScript(Ты)     │        │ PlayerScript(Ты)     │
│ playerName="Вася"    │        │ playerName="Вася"    │        │ playerName="Вася"    │
│ playerColor=красный  │        │ playerColor=красный  │        │ playerColor=красный  │
│ isLocalPlayer=TRUE   │        │ isLocalPlayer=FALSE  │        │ isLocalPlayer=FALSE  │
└──────────────────────┘        └──────────────────────┘        └──────────────────────┘
│ PlayerScript(Игрок2) │        │ PlayerScript(Игрок2) │        │ PlayerScript(Игрок2) │
│ playerName="Петя"    │        │ playerName="Петя"    │        │ playerName="Петя"    │
│ playerColor=синий    │        │ playerColor=синий    │        │ playerColor=синий    │
│ isLocalPlayer=FALSE  │        │ isLocalPlayer=TRUE   │        │ isLocalPlayer=FALSE  │
└──────────────────────┘        └──────────────────────┘        └──────────────────────┘
│ PlayerScript(Игрок3) │        │ PlayerScript(Игрок3) │        │ PlayerScript(Игрок3) │
│ playerName="Коля"    │        │ playerName="Коля"    │        │ playerName="Коля"    │
│ playerColor=зеленый  │        │ playerColor=зеленый  │        │ playerColor=зеленый  │
│ isLocalPlayer=FALSE  │        │ isLocalPlayer=FALSE  │        │ isLocalPlayer=TRUE   │
└──────────────────────┘        └──────────────────────┘        └──────────────────────┘
**Ключевой момент:** Каждый клиент имеет **полные копии всех игроков**, но `isLocalPlayer = true` только для своего персонажа.
### SyncVar - Маркированная переменная, которая автоматически синхронизируется между сервером и всеми клиентами при изменении.
### Важные правила
 - Изменять SyncVar можно ТОЛЬКО на сервере
 - Использовать [Command] методы для изменений
 - Хук вызывается на ВСЕХ клиентах (включая сервер)
 - Не изменять SyncVar напрямую на клиенте
```csharp
[Command]
void CmdSetPlayerName(string newName)
{
    // Изменяем на сервере → Mirror автоматически синхронизирует
    playerName = newName;
}

[Command]
void CmdTakeDamage(int damage)
{
    // Проверка на сервере (античит)
    if (damage > 0 && damage < 100)
    {
        health -= damage;
    }
}
```
## Основные свойства и методы
```csharp
bool isLocalPlayer      // Это Я? (true только для моего персонажа)
bool isServer           // Я сервер?
bool isClient           // Я клиент?
bool hasAuthority       // Имею ли право управлять этим объектом?
NetworkIdentity netId   // Уникальный сетевой ID объекта
```
## Жизненный цикл
```csharp
void OnStartLocalPlayer()   // Вызывается при создании локального игрока
void OnStartServer()        // Вызывается при создании на сервере
void OnStartClient()        // Вызывается при создании на клиенте
```
## Компоненты  Mirror
|Компонент|Назначение|
|---|---|
|`NetworkManager`|Управление подключениями, сценами, игроками|
|`NetworkTransform`|Автоматическая синхронизация позиции/вращения|
|`NetworkAnimator`|Синхронизация анимаций|
|`NetworkIdentity`|Идентификатор сетевого объекта|
|`NetworkStartPosition`|Точка спавна игроков|
