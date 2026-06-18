# Multiplayer Board Game — Unity \& C#

> Board game multiplayer real-time yang dikembangkan menggunakan Unity sebagai proyek Tugas Akhir (Skripsi) di Universitas Sumatera Utara. Dibangun dengan custom TCP networking layer, logika game berbasis turn, dan arsitektur OOP.

\---

## Gambaran Umum

Proyek ini adalah board game digital di mana dua pemain bertanding secara real-time melalui jaringan lokal. Setiap pemain melempar dadu dan menggerakkan token mereka menyusuri jalur waypoint. Pemain pertama yang mencapai akhir jalur akan menang. Game ini dibangun sepenuhnya di Unity menggunakan C# dengan fokus pada desain object-oriented yang bersih dan sinkronisasi state melalui jaringan.

\---

## Fitur

* Multiplayer real-time melalui TCP/IP (jaringan lokal)
* Sistem giliran lempar dadu dan gerak pemain berbasis turn
* Animasi dadu dengan hasil random
* Gerakan pemain yang halus berbasis waypoint
* Pengurutan pesan dari server dengan recovery sinkronisasi di sisi client
* Deteksi kondisi menang dan manajemen game-over state
* Manajemen scene: menu utama → game → hasil

\---

## Tech Stack

|Layer|Teknologi|
|-|-|
|Engine|Unity (2D)|
|Bahasa|C#|
|Networking|TCP/IP via `System.Net.Sockets`|
|Arsitektur|Object-Oriented Programming (OOP)|
|Version Control|Git|

\---

## Arsitektur \& Desain OOP

Proyek ini disusun dari lima class yang mewarisi MonoBehaviour, masing-masing dengan tanggung jawab yang jelas dan terpisah.

```
MonoBehaviour (Unity Engine)
    ├── NetworkScript     — Koneksi TCP, background thread, message queue
    ├── Dice              — Animasi lempar dadu, kirim hasil ke server
    ├── GameControl       — Game state, logika giliran, kondisi menang
    ├── FollowThePath     — Gerakan pemain berbasis waypoint
    └── Menu              — Pemilihan pemain, transisi scene
```

### Encapsulation

Setiap class hanya mengekspos apa yang benar-benar perlu diakses komponen lain, dan menyembunyikan sisanya.

```csharp
// NetworkScript.cs
private string IP = "localhost";         // konfigurasi internal, tidak diekspos
private int port = 54000;

public static Queue<string> messageQueue; // sengaja public — dibaca oleh GameControl
public static TcpClient client;           // dipakai bersama oleh SendMessageToServer
```

```csharp
// FollowThePath.cs
\\\[SerializeField]
private float moveSpeed = 1f;            // terlihat di Inspector, tidak bisa diakses script lain

\\\[HideInInspector]
public int waypointIndex = 0;            // bisa diakses GameControl, disembunyikan dari Inspector
```

### Single Responsibility Principle

Setiap class hanya menangani satu domain:

* `NetworkScript` — menangani semua operasi socket TCP dan mengirim pesan ke message queue yang thread-safe
* `Dice` — menjalankan coroutine lempar dadu dan mengirim hasilnya ke server; tidak tahu apa-apa soal pergerakan
* `FollowThePath` — menggerakkan pemain menyusuri waypoint saat `moveAllowed = true`; tidak tahu apa-apa soal aturan game
* `GameControl` — satu-satunya class yang membaca game state, mengontrol giliran, dan mendeteksi kondisi menang
* `Menu` — hanya menangani pemilihan pemain sebelum game dan pemuatan scene

### Producer-Consumer Pattern (Networking)

Sebuah background thread terus-menerus membaca stream TCP dan memasukkan pesan ke dalam queue. Main thread Unity mengambil dan memproses pesan tersebut satu kali per frame di `Update()`. Pendekatan ini menjaga proses networking agar tidak membebani main thread maupun mengganggu rendering.

```csharp
// NetworkScript.cs — producer (background thread)
public static void MessageReader() {
    while (true) {
        stream.Read(bytes, 0, client.ReceiveBufferSize);
        messageQueue.Enqueue(Encoding.UTF8.GetString(bytes));
    }
}

// GameControl.cs — consumer (main thread, dipanggil setiap frame)
void Update() {
    if (NetworkScript.messageQueue.Count > 0) {
        string message = NetworkScript.messageQueue.Dequeue();
        // parsing dan eksekusi aksi dari pesan
    }
}
```

### Pengurutan Pesan \& Recovery Sinkronisasi

Server menandai setiap pesan keluar dengan nomor urutan. Client melacak counter urutan yang diharapkan (`inboundMessageCounter`) dan mengabaikan pesan yang tidak sesuai urutan. Jika terjadi ketidaksesuaian, client mengirim permintaan sinkronisasi agar server mengirim ulang dari pesan terakhir yang sudah dikonfirmasi.

```csharp
// GameControl.cs
if (messageIndex == inboundMessageCounter) {
    MovePlayer(player);
    inboundMessageCounter++;
} else if (!isSyncing) {
    netObject.SendMessageToServer("s" + inboundMessageCounter + "x");
    isSyncing = true;
}
```

### Static Shared State

`GameControl` mengekspos game state yang dipakai bersama sebagai static member, sehingga class lain bisa membacanya tanpa perlu memegang referensi langsung ke instance `GameControl`. Ini adalah pola umum di Unity untuk menyimpan state global.

```csharp
public static bool gameOver = false;
public static int diceSideThrown = 0;
public static void MovePlayer(int playerToMove) { ... }
public static void SendMovePlayerToServer(int player, int amount) { ... }
```

\---

## Struktur Proyek

```
Assets/
├── Scripts/
│   ├── NetworkScript.cs     — TCP client, background reader thread, message queue
│   ├── Dice.cs              — Coroutine lempar dadu, kirim hasil ke server
│   ├── FollowThePath.cs     — Gerakan pemain berbasis waypoint
│   ├── GameControl.cs       — Game state, manajemen giliran, deteksi menang
│   └── Menu.cs              — Pemilihan pemain sebelum game
├── Resources/
│   └── DiceSides/           — Sprite sisi dadu (dimuat saat runtime via Resources.LoadAll)
└── Scenes/
    ├── Menu                 — Layar pemilihan pemain
    └── Game                 — Scene utama game
```

\---

## Cara Menjalankan

1. Clone repository ini
2. Buka proyek di Unity (disarankan versi 2020.3 LTS atau lebih baru)
3. Jalankan TCP server di `localhost:54000`
4. Buka dua instance game (atau build dan jalankan di dua perangkat dalam jaringan yang sama)
5. Pemain 1 klik **Player 1**, Pemain 2 klik **Player 2**
6. Klik dadu untuk melempar — giliran berganti secara otomatis

\---

## Yang Saya Pelajari

* Merancang arsitektur OOP multi-class di Unity dengan pemisahan tanggung jawab yang jelas
* Mengelola komunikasi antar-thread antara main thread Unity dan background thread pembaca jaringan
* Mengimplementasikan komunikasi socket TCP
* Membangun protokol pengurutan pesan dan recovery sinkronisasi dari nol
* Menyusun game loop yang lengkap: menu → gameplay → win state → restart

\---

## Penulis

**Mhd. Raka Prayudhistira**
Ilmu Komputer — Universitas Sumatera Utara

