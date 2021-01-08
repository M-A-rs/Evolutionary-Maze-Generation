# Evolutionary Level Generation

Procedural level generation using cellular automata and genetic algorithms for Evolutionary Algorithms class. 

Video (portuguese only): https://drive.google.com/file/d/1M4mpXgB_xUn_OzC7zz_lEIlG7I3auaDH/view?usp=sharing

## Algorithm

The evolutionary cellular automata algorithm is based on Adams' publications, while region merging is due to Sebastian Lague (see references). 

A population of levels is stored and their state is updated based on
cellular automata rules. Each level has it's fitness evaluated and then the genetic algorithm is applied (selection, crossover and mutation). This process is repeated for many generations.

### Cellular Automata
The cellular automata rules are applied to each inner cell of the maze.
The ruleset use a 18-bit chromosome and a 8-neighborhood around each cell.
The first half of the chromosome specifies a set of rules for an empty cell
and the second half of the chromosome specifies a set of rules for a filled cell.

### Genetic Algorithm
It's used a highly elitist selection (the 50% best fit levels are kept for the next generation) and a high mutation rate (to increase genetic diversity).
It's used a single point crossover, where all individuals levels are considered to mate.
Finally, mutation is applied to the offsprings.
The process is repeated for many generations.

## Some results


## References
[Daniel Shiffman - Nature of Code: Cellular Automata](https://natureofcode.com/book/chapter-7-cellular-automata/)

[Daniel Shiffman - Nature of Code: The Evolution of Code](https://natureofcode.com/book/chapter-9-the-evolution-of-code/)

[Sebastian Lague - Procedural Cave Generation: Videos](https://www.youtube.com/playlist?list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9)

[Sebastian Lague - Procedural Cave Generation: Code](https://github.com/SebLague/Procedural-Cave-Generation)

[Adams, Parekh and Louis - Procedural level design using an interactive cellular automata genetic algorithm (2017)](https://dl.acm.org/doi/abs/10.1145/3067695.3075614?download=true)

[Adams and Louis - Procedural maze level generation with evolutionary cellular automata (2017)](https://ieeexplore.ieee.org/abstract/document/8285213)