# Module 3 projects
In this module you will work on a turn based solitaire card game and then use the framework provided to implement a different solitaire card game from a selected group of possibilities.



## Prospector
*Gibson-Bond's Prospector Solitaire*

The chapter is split into two main parts. One create cards as a collection of sprite objects, managing their visual hierarchy as required by the engine. And second, implement the gameplay for a version of TriPeaks solitaire, with the peaks inverted.

You are provided with the code from the first half of the chapter, making the card objects and initializing the deck.

The repository provided contains a *blend of code* from the 1st and 2nd edition of the book. 

In the second edition the processes of making the cards was broken out in to separate functions. Parts of MakeCard() were split out to form the functions AddDecorators(), AddPips(), AddFace(), and AddBack(). In the 1st edition the code contained all of these processes within the MakeCard function. This made for a longer, more unweildy function.

If you go through the second edtion of the text, the code will not match up as described in the text, but the cards will all be made and the deck is the same at the end of the process. This should not be an issue, because keep in mind - *you do not need to implement the first half of the chapter. It is provided*

As you go through the project not the difference between what the player sees as the game and how the code manages the internals of the game state. The physical layout of the cards are critical to the gameplay, but a completely different set of datastructures handle the process of determining what is free after a card is played and should be flipped to face up.

## Solitaire Card Game (OtherGame Folder in Assets)
One of the goals Gibson-Bond had in structuring the project in this way was that other card games could be built on top of the framework of the existing cards/Deck mechanism.

We will be setting up a new namespace for this game so that you will not need to undo any of the work you do for Prospector solitaire.

You are going to be choosing to build a solitaire card game from one of the following:
- Pyramid Solitaire
- Poker Solitiare
- Golf Solitaire
- Baker's Dozen
- Clock Solitaire

# Deliverable
At the end of this module we will have a UI scene that allows the player to choose the between Prospector Solitaire and the game you chose to implement.

End of game should drive the player back to the menu to choose to restart.
