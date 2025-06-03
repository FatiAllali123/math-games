# Test Mini-Games

## Overview
The **Test Mini-Games** collection is a set of educational games designed to help students practice essential mathematical concepts across multiple grade levels. The mini-games cover topics ranging from reading numbers to solving real-world problems, with levels tailored to different student needs. These games incorporate interactive avatars, customizable difficulty settings, and speech-based input to enhance the learning experience.

## Important Update
### **New Features & Modifications:**
- **Parameterization of addition, subtraction, multiplication, and division** based on retention and different difficulty levels.
- Operations must be solved **vertically and manually** in the game interface.
- **New interaction mechanisms** such as drag-and-drop for solving operations vertically.
- **2D Focus**: All projects will remain 2D, with no assigned 3D tasks for students.
- **Inspired by Duolingo**: Implementing gamification and interactive learning mechanisms.

### **Key Arithmetic Concepts Considered in Mini-Games**
#### **1. Addition**
- Carrying when sums exceed 9.
- Retaining carried values for the next column.
- Right-to-left solving approach.

#### **2. Subtraction**
- Borrowing when minuend digits are smaller.
- Retaining borrowed values for continuity.
- Step-by-step column-based solving.

#### **3. Multiplication**
- Handling partial products.
- Carrying values for multi-digit multiplication.
- Retaining intermediate products and carries.

#### **4. Division**
- Managing quotients and remainders.
- Implementing long division with step tracking.
- Handling decimal adjustments for small dividends.

---

## Table of Contents
1. [Arithmetic Mini-Games](#arithmetic-mini-games)
2. [Number Mini-Games](#number-mini-games)
3. [System Components](#system-components)
4. [Technical Requirements](#technical-requirements)
5. [Installation Instructions](#installation-instructions)
6. [Gamification Elements](#gamification-elements)
7. [Teacher Dashboard Integration](#teacher-dashboard-integration)
8. [Important Update](#important-update)
9. [Contributing](#contributing)
10. [Contact](#contact)

---

## Arithmetic Mini-Games

**Target Grades**: Grades 2–6

**Levels**:
- Level 2: Single-digit numbers (0–10)
- Level 3 or higher: Multi-digit numbers

**Customization**:
- Number of operations, time limits, difficulty, number size, and correct answers required.

### **1. Find All Possible Compositions for a Target Sum, Subtraction, Multiplication or Division**
- Players must find as many compositions as possible to obtain a given number (e.g., 10).
- They can click the **"Add Another Composition"** button to generate new empty slots (e.g., ` . + . `).
- Players drag digits from the on-screen keyboard to fill in the blanks.

### **2. Solve the Operation Vertically**
- Players solve arithmetic operations step by step, using drag-and-drop mechanics to position numbers correctly.
- Example:
  ```
    1 5
  +
    2 0
  ------
    . .
  ```
- Players drag digits from the keyboard into the solution slots.

### **3. Choose the Right Answer**
- Given a word problem, players must select the correct answer from multiple choices.
- Example:
  _تملك سلوى 30 درهما ومنحها أبوها 20 درهما. كم أصبح لدى سلوى من درهم؟_
  - [ ] 30 + 10
  - [ ] 30 - 20
  - [x] 20 + 30
  - [ ] 30 - 10

### **4. Solve a Multi-Step Word Problem**
- Players must solve a problem requiring multiple operations.
- Example:
  _يشتغل عامل في باخرة لصيد السمك. 10 ساعات في اليوم. وتؤدى له 12 درهما عن كل ساعة عمل. إذا علمت أنه يعمل 26 يوما في كل شهر. فما هي أجرته الشهرية؟_
- Players must:
  1. Click **"Add an Operation"** to choose from Addition, Subtraction, Multiplication, or Division, this will give them the vertical operation structure needed.
  2. Solve each step using the vertical operation structure.
  3. Drag and drop digits into the correct spots.
  4. Click **"Next"** to proceed to the next step and do the same.
  5. Click **"Done"** when all steps are completed.

---

## Number Mini-Games

**Target Grades**: Grades 1–6

**Customization**:
- Number of digits, number of options, test duration, display speed, and number of correct answers.

### **1. Find the Previous and Next Number**
- Players must fill in the missing numbers before and after a given number.
- Example:
  ```
  .  .    25    .  .
  ```
- Players drag digits from the keyboard to complete the sequence.

### **2. Tap the Matching Pairs**
- Players match numbers with their corresponding words.
- Example:
  ```
  35      أربعة وعشرون
  25      خمسة وثلاثون
  ```
- Players tap the correct matching pairs.

### **3. Order the Numbers**
- Players arrange numbers in ascending or descending order.
- Example:
  ```
  673    894    849
                   <     <  
  ```
- Players drag and drop numbers into the correct order.

### **4. Compare Numbers**
- Players place the correct comparison signs between numbers.
- Example:
  ```
  7 . 5      32 . 23      434 . 463
  ```
- Players drag `>` or `<` signs into the correct spots.

### **5. What Number Do You Hear?**
- The game reads a number aloud, and players must compose it using drag-and-drop digits from the keyboard.

### **6. Decompose the Following Number**
- Players must decompose a given number into its components.
- Example:
  ``` 
  67 400 → 60 000 + 7000 + 400
  ```
- Players drag digits from the keyboard to correctly break down the number into place value components.

### **7. Write the Following Number in Letters**
- Players must write a number in words by dragging and dropping Arabic words into the result box.
- Example:
  ``` 
  127 → مئة وسبعة وعشرون
  ```
- The words "وعشرون", "مئة", "وسبعة", and other relevant words will be displayed, and the player needs to drag them in the correct order.

### **8. Identify the Units, Tens, Hundreds, and Thousands**
- Players identify the place value of each digit in a given number.
- Example:
  ``` 
  3117 → 
  3 under آلاف, 
  1 under مئات, 
  1 under عشرات, 
  7 under وحدات
  ```
- The player must drag the correct digits from the keyboard and drop them under the corresponding place value categories (Units, Tens, Hundreds, Thousands).

### **9. Read the Number Aloud**
- Players read a displayed number aloud, reinforcing number recognition and pronunciation skills.

---

## System Components

Thanks Najlae — the full README is shaping up beautifully! Here's a final suggestion to seamlessly blend your additions for the Unity version into your existing `README.md`. This ensures clarity for both Unreal and Unity student contributors while preserving the project's pedagogical and technical richness.

---

### ✅ Final Merge Suggestion for `README.md`

After the **Unreal Engine System Components** section (`## System Components`), add a new heading:

---

## 🧩 Unity System Components (2025+ Version)

### ⚙️ Existing Classes & Structures

#### Core Data & Profile System
- **`PlayerProfile.cs`** – Tracks progress, rewards, and "skillsToImprove".
- **`RewardData.cs`** – Stores gamification scores, rank, XP-like points.
- **`GameProgressEntry.cs`** – Best/latest score tracking per mini-game.
- **`AchievementData.cs`** – Earned badge storage.
- **`UserData.cs`** – Firebase-serializable container for syncing with `users/<uid>`.

#### Firebase Integration
- **`FirebasePlayerDataManager.cs`** – Load/save player data using Firebase Realtime Database.
- Works with `FirebaseAuth` and `FirebaseDatabase`.

#### UI & Interaction (Ready-to-Use Prefabs)
- **`KeyboardWidget.cs`** – Renders a keyboard of symbols.
- **`KeyboardButton.cs`** – Draggable math symbols.
- **`GhostButtonController.cs`** – Visual ghost during drag.
- **`DigitSlot.cs`** – Drop areas to input answers.

📦 Prefabs:
- `KeyboardGrid`, `GhostButtonCanvas`, `DigitSlotCanvas`.

---

### 🖥️ Unity Technical Requirements

- **Unity Version:** Unity 6 (LTS recommended).
- **Dependencies:**
  - Firebase Unity SDK: Auth + Realtime Database
- **Platforms:**
  - Android 
  - Windows (editor/tested)

🛠️ Setup Recommendations:
- Use prefabs like `KeyboardGrid`, `DigitSlotCanvas`, and `GhostButtonCanvas` under a Unity UI Canvas.
- Call `RewardSystemManager.Instance.AddSuccess()` in mini-game scripts to reward correctly.
- Inherit your mini-games from a base class (e.g., `MathoriaMiniGameWidget`) to enable reusable UI logic.

---

## Gamification Elements

- **Rewards**: GameLevel (XP) increases after completing the test. Score accumulates after each mini-game, and the final score is displayed after completing the test along with the real-math grade level of the player.
- **Progress Tracking**: Visual progress bars and level indicators.
- **Animations**: Fun transitions and celebratory effects for correct answers.
- **Avatars**: Provide verbal guidance, feedback, and motivation.

---

## Teacher Dashboard Integration

- **Customization of Test Parameters**: Teachers can customize test difficulty, duration, problem count, and the number of correct answers required.
- **Real-Time Monitoring of Student Performance**: Track student performance in real-time while they are engaging with the mini-games.
- **Analytics & Reports**: Teachers have access to detailed analytics, showing student progress, areas of strength, and areas for improvement. Reports can be generated based on student performance, helping educators adjust teaching strategies as needed.
- **Link to GitHub Repo**: [GitHub Repository for Teacher Dashboard](https://github.com/najlae01/math-web.git)

---

## Important Update 
We are making a key change to how students/players create and access their game accounts. Going forward, students will not be able to create accounts themselves—only teachers or school administrators can do so.

To streamline this process, we are removing the in-game authentication form. Instead, students will log in using a QR code provided by their teacher upon account creation.

In the future, we plan to introduce fingerprint authentication as an option for compatible devices.

---

## Installation Instructions

1. **Clone the Repository**  
   Open a terminal and run:
   ```bash
   git clone https://github.com/najlae01/math-mini-games-unity.git
   ```

2. **Create a Firebase Project**  
   - Go to the [Firebase Console](https://console.firebase.google.com/).
   - Create a new project (e.g., "Math Test Game").

3. **Register Android and Web Apps**  
   - In the Firebase project settings, add both an **Android app** and a **Web app**.
   - For Android, make sure to add the correct package name and SHA certificate.

4. **Enable Authentication Methods**  
   - In the **Authentication** tab, enable the **Email/Password** sign-in method.

5. **Set Up Realtime Database**  
   - In the **Realtime Database** section:
     - Create a new database.
     - Choose test mode or set rules as appropriate.

6. **Download `google-services.json`**  
   - From **Project Settings > General**, scroll to your Android app.
   - Download the `google-services.json` file.

7. **Import Firebase SDK into Unity**
   - Delete the placeholder `Assets/Firebase` folder if it exists.
   - Download the [Firebase Unity SDK](https://firebase.google.com/docs/unity/setup).
   - Unzip and import **`FirebaseDatabase.unitypackage`** via:
     ```
     Unity Editor → Assets → Import Package → Custom Package...
     ```

8. **Place `google-services.json`**  
   - Copy it into the root of your Unity project (same level as `Assets/`).

9. **Run the Game**  
   - After a successful compilation, press **Play** in the Unity Editor or build for Android.

---

## Contributing

We welcome contributions to improve these mini-games!

1. Fork the repository.
2. Create your group branch (`git checkout -b group-one`).
3. Commit your changes (`git commit -am 'Add group-one'`).
4. Push to your branch (`git push origin group-one`).
5. Open a pull request with detailed descriptions of your changes.

---

## Contact

For support, contact [Najlae](mailto:najlae.abarghache@etu.uae.ac.ma).

#   m a t h - g a m e s  
 