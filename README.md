# Night Shift: Old Man AI System

Night Shift is a game revolving around a cute little shadow monster that wishes to help an elderly janitor complete his nightly duties. The creature has good intentions, but they’d better be careful. The Old Man is frail and scares easily...

The game features only the player and one other entity: the Old Man. This repo contains the source code that the game uses to control the Old Man's AI.

The system contains 3 main parts:

1. The Puzzle Manager - This singleton class manages the Old Man and his place in the overarching puzzle inside the game world. It contains the priority queue of actions the old man can perform and assigns those actions to the old man himself.

2. The Old Man - This is the actual Old Man. It holds and executes the current action he should perform as well as some control for "thought bubbles" which can show what the old man's current goal is.

3. The Action Classes - The action classes are a set of classes that contain the specific execution for each action type. These types include "StandStill", "MoveTo", "Path", "NearPlayer" (death condition), and "Animation". They are all children of one parent class that contains the base functionality for every action.

The PuzzleManager has a priority queue of actions, and assigns the action with the lowest prioirty to the Old Man for him to execute. Once that action is done or an action with a lower priority appears, the PuzzleManager assigns a new action to the Old Man. The actions have a couple of base features: the ability to discard an action after it spends a certain amount of time in the queue, as well as the ability to create a visual thought bubble on the old man to visually communicate what he is doing.

This system allows the game to assign new actions for a variety of different contexts. One example is when the player stands in light or shakes an object, the Old Man moves there then goes back to what he was doing beforehand. This action can be interupted if the player shakes a different object, putting the first MoveTo back into the queue and assigning the new one to the Old Man.
