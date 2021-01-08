import matplotlib as mpl
import matplotlib.pyplot as plt
import numpy as np

def create_graph(title):

    plt.style.use('seaborn')

    plt.title(title)


    plt.xlabel("generations")
    plt.ylabel("fitness")

    # for 0 to 120:
    x = [i for i in range(121)]

    # average fitness of population list:
    y_average = []
    # best individual fitness in population:
    y_best = []

   
    with open("teste120sumAndDead.txt", 'r+', encoding="utf-8") as f:
        
        line = f.readline() # ignore header
        
        line = f.readline()

        while line:
            line = line.split()

            numbers_list = []

            for word in line:
                number = float(word)
                numbers_list.append(number)

            # append the current average fitness score in the file line:
            y_average.append(numbers_list[1])
            # append the best fitness in the current file line:
            y_best.append(numbers_list[2])

            line = f.readline()

        f.close()


   



    plt.plot(x, y_best, label = "best individual", color = "coral", marker = ".", linestyle = "--", markersize = "10")

    plt.plot(x, y_average, label = "average population", color = "aqua", marker = ".", linestyle = "dotted", markersize = "10")


    plt.legend()

    plt.show()

if __name__ == '__main__':

    create_graph("F3 Performance")