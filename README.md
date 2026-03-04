# Rich Man – 3D Turn-Based Board Game Prototype

## Project Overview

Rich Man is a simplified 3D board game prototype inspired by the classic Monopoly game.  
The project was developed using the Unity game engine as part of the CMP-6056B Game Development coursework.

The game demonstrates several core gameplay systems including player movement, property purchasing, AI decision making and a dynamic camera system.

The objective of the game is to move around the board, purchase properties and collect rent from other players in order to accumulate the most money.

---

## Game Features

The prototype includes several gameplay systems:

• Turn-based dice movement  
• Procedurally generated board tiles  
• Property ownership system  
• AI decision making opponent  
• Dynamic camera system using Cinemachine  
• UI event system for player interaction  

---

## Gameplay Mechanics

### Player Movement

The player movement system is implemented using a step-based movement system.

When a player presses the **Space key**, the dice is rolled and the player moves across the board tile by tile.  
The PlayerMover script controls the movement by interpolating between tile positions.

A small jump animation is included to improve the visual feedback during movement.

---

### Property System

When a player lands on an unowned property tile, the game will display a UI panel asking whether the player wants to purchase the property.

If the player purchases the property:

• The player’s money will decrease  
• The tile becomes owned by that player  
• The tile colour changes to represent ownership  

If another player lands on that tile later, rent will automatically be deducted and transferred to the owner.

This introduces strategic decision making into the gameplay.

---

### AI Decision System

The game includes an AI opponent that automatically plays its turn.

The AI performs the following actions:

• Rolls the dice automatically  
• Moves across the board  
• Decides whether to purchase properties  

The AI checks whether it has enough remaining money before purchasing a property.  
This allows the game to function without requiring multiple human players.

---

### Camera System

The game uses the **Cinemachine camera system** to create a dynamic third-person perspective.

A Target Group is used to track both the Player and AI characters.  
The camera automatically adjusts its position to keep both characters visible during gameplay.

This improves the visibility of gameplay events and creates a more cinematic presentation.

---

## Controls

| Input | Action |
|------|------|
Space Key | Roll Dice |
Mouse Click | Buy Property |
Mouse Click | Skip Purchase |

---

## Project Structure

---

## Development Tools

Game Engine  
Unity 6000.2.15f1

Programming Language  
C#

Camera System  
Cinemachine

---

## Future Improvements

Possible improvements for the game include:

• Multiple AI opponents  
• Property upgrades such as houses and hotels  
• Random event cards  
• Multiplayer support  
• Improved models and animations  

---

## Author

Student Name  
Chenyihan Wang  

Module  
CMP-6056B Game Development  

University  
University of East Anglia

---

## Repository

GitHub Repository Link:

https://github.com/leave-wang/RichMan-Unity-Game
