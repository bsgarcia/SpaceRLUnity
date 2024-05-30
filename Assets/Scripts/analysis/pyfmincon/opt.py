# Written by Andrew Ning.  Feb 2016.
# FLOW Lab, Brigham Young University.
# Adapted by B. Garcia Dec 2018
import numpy as np


try:
    import matlab.engine
    import matlab
except ImportError:
    import warnings
    warnings.warn("""
    Matlab engine not installed.
    Instructions here: http://www.mathworks.com/help/matlab/matlab_external/install-the-matlab-engine-for-python.html

    If still having problems, try setting DYLD_FALLBACK_LIBRARY_PATH to contain your python lib location.
    See: http://www.mathworks.com/matlabcentral/answers/233539-error-importing-matlab-engine-into-python
    """)


def start():
    global eng
    eng = matlab.engine.start_matlab()


def stop():
    global eng
    eng.quit()
    del eng


def new_engine():
    return matlab.engine.start_matlab()


def connect():
    global eng
    try:
        eng.eval('disp("Already connected to a MATLAB session!")', nargout=0)

        return
    except (matlab.engine.MatlabExecutionError, NameError):
        pass

    try:
        # Try to connect to an existing MATLAB session
        eng = matlab.engine.connect_matlab()
    except Exception as e:
        print(f"Failed to connect to an existing MATLAB session: {e}")
        return None


def fmincon(function, x0, lb, ub, nonlcon=[], A=[], b=[], Aeq=[],
            beq=[], options={}, providegradients=False, optional_args=[], engine=None):
    global eng
    if engine is not None:
        if 'eng' in globals():
            del eng
        eng = engine
        
    # convert to numpy array then list then to matlab type
    # these first conversions are necessary to allow both numpy and list style inputs
    x0 = matlab.double(np.array(x0).tolist())
    ub = matlab.double(np.array(ub).tolist())
    lb = matlab.double(np.array(lb).tolist())
    A = matlab.double(np.array(A).tolist())
    b = matlab.double(np.array(b).tolist())
    Aeq = matlab.double(np.array(Aeq).tolist())
    beq = matlab.double(np.array(beq).tolist())
    
    for i in range(len(optional_args)):
        x = optional_args[i]

        match x:
            case np.int64():
                optional_args[i] = matlab.int64(x)
            case  np.ndarray():
                if x.dtype == np.bool8:
                    optional_args[i] = matlab.logical(x.tolist())
                elif x.dtype in (np.int64, np.int32, np.int16, np.int8):
                    optional_args[i] = matlab.int64(x.tolist())
                elif x.dtype in (np.float64, np.float32, np.float16):
                    optional_args[i] = matlab.double(np.array(x).tolist())
            case list():
                optional_args[i] = matlab.double(x)
            case int():
                optional_args[i] = matlab.int16(x)
            case str():
                optional_args[i] = eng.matlab.string(x)
            case bool():
                optional_args[i] = matlab.logical(x)
            # handle pandas dataframes and series
            # case ('pandas' in str(type(x))):
                # optional_args[i] = matlab.double(x.values.tolist())
            case _:
                print('Warning: type not recognized for optional argument ' + str(i) + '.')
                print('Type is: ' + str(type(x)))

    # print('Running fmincon...')
    # run fmincon
    xopt, ll, exitflag = eng.optimize(function, x0, A, b,
        Aeq, beq, lb, ub, nonlcon, options, providegradients, optional_args, nargout=3)

    return np.squeeze(xopt), ll

