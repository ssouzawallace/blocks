import random
class Greeter():
    def __init__(self, name):
        self.name = name
    def greet(self):
        return "Hi, " + self.name
    def random_number(self, start, end):
    	return random.randint(start, end)