# Rich Man – 3D Board Game Prototype

## Project Overview

This project is a prototype of a 3D turn-based board game inspired by the classic Monopoly game.  
The game was developed using the Unity Engine as part of the CMP-6056B Game Development coursework.

The prototype demonstrates several core gameplay systems including player movement, property ownership mechanics, AI decision-making and a dynamic camera system.

---

## Game Features

• Procedurally generated board tiles  
• Turn-based dice movement system  
• Property purchase and rent mechanics  
• AI opponent decision system  
• Cinemachine dynamic camera system  
• Interactive UI event system  

---

## Game Mechanics

### Player Movement
Players roll a dice and move step-by-step across the board tiles.  
The movement system includes interpolation and jump animation for smoother visual feedback.

### Property System
When landing on a property tile, players can purchase the property.  
If another player lands on that tile later, rent will be automatically deducted.

### AI System
The AI player automatically performs actions including:

- Rolling the dice
- Moving across the board
- Deciding whether to purchase properties

The AI decision system evaluates the available money before making a purchase decision.

---

## Controls

| Input | Action |
|------|------|
Space Key | Roll Dice |
Mouse Click | Buy Property |
Mouse Click | Skip Purchase |

---

## Development Tools

Unity Version  
