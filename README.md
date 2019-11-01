# Just-Another-Platformer

Intro:
A 2D game written in C# using Monogame library.
The game is a classic platform game, composes of levels, where each 6 levels are a 'world'
with its own unique enemies and look.

Each world has a final level which is a 'Boss' level, and is different then the normal platform level
(The boss levels ressemble the dino game in google chorme when the wifi connection dies).

The game is not finished and is still a work in progress. It contains a few minor bugs,
and require additional art (There are a few missing sound effects and animations), and further development 
(more levels etc.).

AI:
The boss level's AI is uses a neural network, built and trained using 
a cool version of Neuro Evolution called NEAT (More info on NEAT here https://towardsdatascience.com/neat-an-awesome-approach-to-neuroevolution-3eca5cc7930f).
The implemntation of the NEAT algorithm as it appears in the game, is a heavily 
modified version of this implementation: https://github.com/Atharv24/NEAT .

The project also contains a vanilla implemetation of a neural network and a 
Nero Evolution training procces, which I implemented all by myself. 
The vanilla algorithm worked perfectly, but I decided to use NEAT, instead of the 
vanilla Neuro Evolution algorithm, simply because it wascooler and more interesting, 
and I really wanted to experiment with this neat (pun intended) algorithm.

Misc:
This project was my final project as a part of my BAGRUT (Isreali version of final exams)
in Software engineering, and was credited with the maximal grade of 100 out of 100.
