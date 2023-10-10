import pyfmincon.opt
import numpy as np
print('Running test.py...')

pyfmincon.opt.connect()

r = np.random.normal(0.8, .01, 49).tolist()
pyfmincon.opt.fmincon(
    function='fit.fit',
    x0=[1e1, 1e1],
    optional_args=[49, [0, ] * 49, [0, ]*49, r, [1, ] * 49, [-1] * 49, [1, ] *49],
    lb=[1e1, 1e1],
    ub=[1e5, 1e5],
    options={},
)