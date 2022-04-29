# Prototype-Prospector_Solitaire

This project is a Unity Example Game from the book [Introduction to Game Design, Prototyping, and Development, 2nd Edition](https://book.prototools.net/), by Jeremy Gibson Bond.

It is a card game prototype, inspired by the [Tri-peaks solitaire game](https://en.wikipedia.org/wiki/Tri_Peaks_(game)).

You can play it [here!](https://shaman37.itch.io/prototype-prospector-solitaire)
## How to play

![Prospector](https://user-images.githubusercontent.com/17680666/165343805-8324ff3d-4930-47c2-94f9-3cf30b6754e3.png)

- You may move any face-up card in the tableau that is a rank abovce or below the target card, by clicking on the desired card to play. When you play a card it becomes the new target card.
- Card Rank hierarchy is as follows:
  - A -> 2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 8 -> 9 -> 10 -> J -> Q -> K
- Aces and Kings wrap around, meaning that if a King is the target card, you can play an Ace and vice-versa.
- A face-down card is revealed if the 2 face-up cards above said card are played.
- A player can click the draw pile to fetch a new target card.
- If the tableau is emptied before the draw pile, then you win the game, else you lose.

## Scoring

- Moving a card from the tableau to the target card is worth 1 point.
- Every subsequent card moved from the tableau, without drawing from the draw pile, increases the card score by 1. So a run of 5 cards played from the tableau results in a score chain of 1 + 2 + 3 + 4 + 5, worth a total of 15 points.
- If the player wins the round, her score is carried over to the next round.
- If the player loses the round, her score from all round is totaled and checked agains the current high score.

## Example

https://user-images.githubusercontent.com/17680666/165349948-b55bcf08-0a8b-4ae4-ab5a-16429dfbb0c7.mp4
