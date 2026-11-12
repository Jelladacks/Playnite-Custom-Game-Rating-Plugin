<div align="center">

<img src="CustomGameRating/icon.png" width="96" alt="Custom Game Rating icon" />

# Custom Game Rating

**내 기준으로 게임을 평가하고, 나만의 평가 대시보드를 만들어 보세요.**<br/>
**Rate games by your own criteria and build your personal rating dashboard.**

[![Release](https://img.shields.io/github/v/release/Jelladacks/Playnite-Custom-Game-Rating-Plugin?label=release)](https://github.com/Jelladacks/Playnite-Custom-Game-Rating-Plugin/releases)
![Playnite](https://img.shields.io/badge/Playnite-10-blue)
![Language](https://img.shields.io/badge/lang-한국어%20%7C%20English-green)

</div>

---

## 📖 소개 / Introduction

Playnite에 **나만의 평가 기능**과, 내 평가를 통계로 보여주는 **대시보드 탭**을 추가하는 플러그인과 테마입니다.

A Playnite plugin and theme that add a **custom rating feature** and a **dashboard tab** showing statistics of your ratings.

<p align="center">
  <img width="860" alt="Dashboard" src="https://github.com/user-attachments/assets/da0ffe79-5fd6-4b3d-9eea-62b1735c292f" />
</p>

---

## ✨ 주요 기능 / Features

| 기능 | Feature |
| --- | --- |
| 🎯 직접 정한 평가 항목으로 점수 매기기 | Score games with your own rating categories |
| 📝 항목별 점수와 한 줄 리뷰 작성 | Per-category scores and a written review |
| ⭐ 게임 상세 화면에 리뷰 / 게임 목록에 총점 표시 | Review in game details, total score in the game list |
| 📊 평가 통계 대시보드 (최고·최저·최근 평가, 선호 게임, 항목별 성향 등) | Statistics dashboard (highest / lowest / latest, favorites, category tendencies, …) |
| 💾 평가 데이터 백업 및 복원 | Backup & restore all rating data |
| 🌐 한국어 / English 지원 | Korean / English support |

---

## 📦 설치 / Installation

1. [Releases](https://github.com/Jelladacks/Playnite-Custom-Game-Rating-Plugin/releases/tag/Plugin) 페이지에서 두 파일을 받습니다.<br/>
   Download both files from the [Releases](https://github.com/Jelladacks/Playnite-Custom-Game-Rating-Plugin/releases/tag/Plugin) page.

   | 파일 / File | 설명 / Description |
   | --- | --- |
   | `plugin_x.x.pext` | 플러그인 (필수) · Plugin (required) |
   | `theme_x.x.pthm` | 리뷰·총점을 화면에 보여주는 테마 · Theme that displays reviews and scores |

2. 두 파일을 더블클릭하거나 Playnite 창에 끌어다 놓아 설치합니다.<br/>
   Double-click each file, or drag and drop it onto the Playnite window.

3. Playnite를 재시작합니다.<br/>
   Restart Playnite.

> [!NOTE]
> 테마 없이 플러그인만 설치해도 평가 작성과 통계 대시보드는 사용할 수 있습니다. 게임 화면에 리뷰와 총점을 표시하려면 테마가 필요합니다.<br/>
> The plugin alone supports writing ratings and the dashboard. The theme is needed to show reviews and scores on game screens.

---

## ⚙️ 설정 / Settings

왼쪽 위 Playnite 아이콘 → **Add-ons...** → **Extensions settings** → **Generic** → **Custom Game Rating** 으로 이동합니다.

Go to the Playnite icon (top-left) → **Add-ons...** → **Extensions settings** → **Generic** → **Custom Game Rating**.

<p align="center">
  <img width="646" alt="Settings" src="https://github.com/user-attachments/assets/1affe0ce-5938-4a10-ab55-2f3781f36992" />
</p>

### 평가 항목 / Categories

상단의 **Category** 목록에서 나만의 평가 기준을 추가·삭제·정렬할 수 있습니다.

Add, remove, and reorder your own rating criteria in the **Category** list at the top.

<p align="center">
  <img width="644" alt="Categories" src="https://github.com/user-attachments/assets/d24e7ce5-9216-4ce9-9c75-47fe45c5e7ad" />
</p>

### 백업 / Backup

하단에서 모든 평가 데이터를 `.json` 파일로 **백업**하고 **복원**할 수 있습니다.

At the bottom, you can **back up** and **restore** all rating data as a `.json` file.

> [!WARNING]
> 복원하면 현재 평가 데이터가 백업 파일의 내용으로 바뀝니다.<br/>
> Restoring replaces your current rating data with the contents of the backup file.

---

## 📝 평가하기 / Rating a Game

플레이 버튼 아래의 **＋** 버튼을 누르거나,

Click the **＋** button below the Play button,

<p align="center">
  <img width="284" alt="Plus button" src="https://github.com/user-attachments/assets/d21f8ec0-cf2c-499f-b28b-04ae55198c7f" />
</p>

**More** 버튼 메뉴에서 평가 메뉴를 선택합니다.

or choose the rating item from the **More** menu.

<p align="center">
  <img width="293" alt="More menu" src="https://github.com/user-attachments/assets/6ad667bf-a93e-4742-be9d-4e2bf6359c24" />
</p>

평가 창에서 항목별 점수와 리뷰를 작성하고 저장합니다.

Fill in per-category scores and your review, then save.

<p align="center">
  <img width="760" alt="Review window" src="https://github.com/user-attachments/assets/adc93b83-3a1c-48fc-9433-47d15053f73b" />
</p>

---

## 👀 리뷰 보기 / Viewing Reviews

테마가 설치되어 있다면 게임 상세 화면에서 내 리뷰를 볼 수 있습니다.

With the theme installed, your review appears in the game details view.

<p align="center">
  <img width="760" alt="Review in details" src="https://github.com/user-attachments/assets/14267348-f9dd-40a4-995f-5276697069d3" />
</p>

항목 점수로 계산된 총점은 게임 목록에서도 간단히 확인할 수 있습니다.

The total score, calculated from category scores, is also shown in the game list.

<p align="center">
  <img width="354" alt="Score badge" src="https://github.com/user-attachments/assets/c81a20a2-333e-4d36-a69a-03ab712d27b2" />
</p>

---

## 📊 평가 대시보드 / Rating Dashboard

이 플러그인의 가장 큰 장점은 **나만의 평가 대시보드**입니다. 왼쪽 사이드바의 탭에서 열 수 있습니다.<br/>
내 취향과 평가 기준을 통계로 다시 보고, 직접 게임 평론가가 되어 보세요!

The highlight of this plugin is **your personal rating dashboard**, opened from the sidebar tab.<br/>
Look back on your taste and criteria through statistics, and become your own game critic!

<p align="center">
  <img width="860" alt="Dashboard" src="https://github.com/user-attachments/assets/da0ffe79-5fd6-4b3d-9eea-62b1735c292f" />
</p>

<p align="center">
  <img width="500" alt="Dashboard detail" src="https://github.com/user-attachments/assets/bca2beb7-81ea-407e-a2c6-ec546716b72f" />
</p>

---

## 🎨 테마 제작자용 / For Theme Developers

다른 테마에서도 이 플러그인의 화면 요소를 사용할 수 있습니다.

You can use this plugin's UI elements in your own theme.

```xml
<!-- 리뷰 패널 / Review panel -->
<ContentControl x:Name="CustomGameRating_GameReviewControl" />

<!-- 총점 배지 / Total score badge -->
<ContentControl x:Name="CustomGameRating_GameRatingBadgeControl" />
```

---

## 💾 데이터 저장 위치 / Data Location

평가 데이터는 Playnite의 확장 데이터 폴더에 저장됩니다.

Rating data is stored in Playnite's extension data folder.

```
<Playnite data>\ExtensionsData\447bc65f-ca7a-4b9d-8080-63d1b40e4e41\customRating.json
```

- 설치형 / Installed: `%AppData%\Playnite`
- 포터블 / Portable: Playnite 설치 폴더 / Playnite install folder

---

## 🛠️ 직접 빌드하기 / Building from Source

```bash
cd CustomGameRating
dotnet build -c Release
"<Playnite folder>/Toolbox.exe" pack "bin/Release/net462" "../Releases"
```

- .NET Framework 4.6.2 · PlayniteSDK 6.18.0

---

## 🐞 문제 제보 / Issues

버그나 제안은 [Issues](https://github.com/Jelladacks/Playnite-Custom-Game-Rating-Plugin/issues)에 남겨 주세요.

Please report bugs or suggestions in [Issues](https://github.com/Jelladacks/Playnite-Custom-Game-Rating-Plugin/issues).

---

<div align="center">

내 평가를 남기며 게임 관리의 소소한 재미를 느끼시길 바랍니다. 🎮<br/>
I hope you enjoy the little joys of managing your game library with your own ratings. 🎮

</div>
