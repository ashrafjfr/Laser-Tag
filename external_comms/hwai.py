import numpy as np

def get_threshold(sequence):
    max_acc_x = np.max(np.abs(np.diff(sequence['Acc_X'])))
    max_acc_y = np.max(np.abs(np.diff(sequence['Acc_Y'])))
    max_acc_z = np.max(np.abs(np.diff(sequence['Acc_Z'])))

    return sum([max_acc_x, max_acc_y, max_acc_z])
