# Game state object

class GameState:
    def __init__(self):
        self.p1 = Player()
        self.p2 = Player()

    def get_dict(self):
        data = dict()
        data['p1'] = self.p1.get_dict()
        data['p2'] = self.p2.get_dict()
        return data



class Player:
    def __init__(self):   
        # initialise player
        self.hp = 100
        self.bullets = 6
        self.grenades = 2
        self.shield_hp = 0
        self.shields = 3
        self.deaths = 0

    def __str__(self):
        return str(self.get_dict())

    def get_dict(self):
        data = dict()
        data['hp'] = self.hp
        data['bullets'] = self.bullets
        data['grenades'] = self.grenades
        data['shield_hp'] = self.shield_hp
        data['deaths'] = self.deaths
        data['shields'] = self.shields
        return data
