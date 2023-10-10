import numpy as np
from models import NormativeEV, NormativeLogRatio


def fit(x0, *args):
    print('Running fit...')
    ntrials, s, a, r, destroy, ff1, ff2 = args[0]
    ff_values = np.linspace(-1, 1, 10).round(1)
    try:
        _ = len(x0)
    except:
        x0 = [x0]
        
    if len(x0) == 1:
        temp = x0[0]
        m = NormativeEV(temp=temp, x=ff_values)
    else:
        perceptual_temp = x0[0]
        rl_temp = x0[1]
        m = NormativeLogRatio(perceptual_temp=perceptual_temp, rl_temp=rl_temp, x=ff_values)
    
    ll = 0

    for t in range(ntrials):
        
        ff_chosen = [ff1[t], ff2[t]][a[t]]
        m.learn_perceptual(ff_chosen, destroy[t])

        if destroy[t]:
            m.learn_value(s[t], a[t], r[t])

        ll += np.log(m.ll_of_choice(ff1[t], ff2[t], s[t], a[t]))

    return -ll
