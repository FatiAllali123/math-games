# 🎮 Mathoria — Educational Math Mini-Games

> An interactive mobile game platform that helps primary school students (Grades 1–6) master mathematics through gamified exercises, drag-and-drop mechanics, and real-time teacher monitoring.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Mini-Games](#mini-games)
- [Teacher Dashboard](#teacher-dashboard)
- [Gamification](#gamification)
- [Contributing](#contributing)
- [Contact](#contact)

---

## 📖 Overview

**Mathoria** is an educational platform combining a Unity-based mobile game with an Angular web dashboard for teachers. Students practice arithmetic and number concepts through interactive mini-games inspired by Duolingo's gamification model. Teachers can customize difficulty, monitor progress in real-time, and generate performance reports.

---

## ✨ Features

- 🧮 **Arithmetic mini-games** — addition, subtraction, multiplication, division (vertical solving with drag-and-drop)
- 🔢 **Number concept games** — ordering, comparing, decomposing, reading numbers aloud
- 👩‍🏫 **Teacher dashboard** — real-time monitoring, test customization, analytics & reports
- 🏆 **Gamification** — XP points, level progression, badges, and animated avatars
- 🔐 **Secure login** — QR code-based student authentication (no self-registration)
- ☁️ **Firebase integration** — real-time data sync across devices
- 📱 **Mobile-first** — built for Android with Unity 6

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|------------|
| Mobile Game | Unity 6 (C#) |
| Web Dashboard | Angular + TypeScript |
| Backend / Auth | Firebase Authentication |
| Database | Firebase Realtime Database |
| UI Design | Figma |
| Version Control | Git & GitHub |

---

## 📁 Project Structure

```
math-games/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── PlayerProfile.cs        # Progress, rewards, skills tracking
│   │   │   ├── RewardData.cs           # XP, rank, gamification scores
│   │   │   ├── GameProgressEntry.cs    # Best/latest score per mini-game
│   │   │   ├── AchievementData.cs      # Earned badges
│   │   │   └── UserData.cs             # Firebase-serializable user container
│   │   ├── Firebase/
│   │   │   └── FirebasePlayerDataManager.cs
│   │   └── UI/
│   │       ├── KeyboardWidget.cs       # Math symbol keyboard
│   │       ├── KeyboardButton.cs       # Draggable digit buttons
│   │       ├── GhostButtonController.cs
│   │       └── DigitSlot.cs            # Drop zones for answers
│   └── Prefabs/
│       ├── KeyboardGrid
│       ├── GhostButtonCanvas
│       └── DigitSlotCanvas
├── Packages/
└── ProjectSettings/
```

---

## 🚀 Getting Started

### Prerequisites

- Unity 6 (LTS)
- Firebase Unity SDK (Auth + Realtime Database)
- Android SDK (for mobile builds)

### Installation

**1. Clone the repository**
```bash
git clone https://github.com/FatiAllali123/math-games.git
cd math-games
```

**2. Set up Firebase**
- Create a project on [Firebase Console](https://console.firebase.google.com/)
- Add an Android app with your package name + SHA certificate
- Enable **Email/Password** authentication
- Create a **Realtime Database** (test mode or custom rules)
- Download `google-services.json` and place it at the root of the Unity project

**3. Import Firebase SDK**
```
Unity Editor → Assets → Import Package → Custom Package
```
Import `FirebaseDatabase.unitypackage` from the [Firebase Unity SDK](https://firebase.google.com/docs/unity/setup).

**4. Run**

Press **Play** in the Unity Editor, or build for Android via `File → Build Settings`.

---

## 🎯 Mini-Games

### 🔢 Arithmetic Games (Grades 2–6)

| Game | Description |
|------|-------------|
| **Find Compositions** | Find all number compositions for a target (e.g., ways to make 10) |
| **Solve Vertically** | Solve operations step-by-step using drag-and-drop in vertical format |
| **Choose the Answer** | Pick the correct operation for a word problem |
| **Multi-Step Problem** | Solve real-world problems requiring multiple operations |

### 🔡 Number Games (Grades 1–6)

| Game | Description |
|------|-------------|
| **Previous & Next** | Fill in the missing numbers in a sequence |
| **Matching Pairs** | Match numbers with their word equivalents (Arabic) |
| **Order Numbers** | Arrange numbers in ascending or descending order |
| **Compare Numbers** | Place `>` or `<` between numbers |
| **What Number Do You Hear?** | Compose a number after hearing it read aloud |
| **Decompose a Number** | Break a number into place-value components |
| **Write in Letters** | Drag Arabic words to write a number in letters |
| **Place Value** | Identify units, tens, hundreds, thousands |
| **Read Aloud** | Reinforce number reading and pronunciation |

---

## 👩‍🏫 Teacher Dashboard

🔗 [Dashboard Repository](https://github.com/najlae01/math-web.git)

- **Customize tests** — difficulty, duration, number of problems, correct answers required
- **Monitor in real-time** — track student sessions as they happen
- **Analytics & reports** — view progress, strengths, and areas for improvement
- **Create accounts** — only teachers/admins can create student accounts (QR code login)

---

## 🏆 Gamification

- **XP System** — points awarded after each correct answer
- **Level Progression** — GameLevel increases as XP accumulates
- **Final Score** — displayed after test completion with real math-grade level
- **Badges** — unlocked through achievements
- **Animated Avatars** — guide, encourage, and give feedback to students
- **Celebratory Animations** — triggered on correct answers and level-ups

---

-
