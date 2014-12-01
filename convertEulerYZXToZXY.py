# Python code extracted from relax:  http://svn.gna.org/svn/relax/1.3/maths_fns/rotation_matrix.py

# Python module imports.
from copy import deepcopy
from math import acos, asin, atan2, cos, pi, sin, sqrt, degrees, radians
from numpy import array, cross, dot, float64, hypot, sign, transpose, zeros
from numpy.linalg import norm
from random import gauss, uniform
import eulerAngles

# Global variables.
EULER_NEXT = [1, 2, 0, 1]    # Used in the matrix_indices() function.
EULER_TRANS_TABLE = {
        'xzx': [0, 1, 1],
        'yxy': [1, 1, 1],
        'zyz': [2, 1, 1],

        'xzy': [0, 1, 0],
        'yxz': [1, 1, 0],
        'zyx': [2, 1, 0],

        'xyx': [0, 0, 1],
        'yzy': [1, 0, 1],
        'zxz': [2, 0, 1],

        'xyz': [0, 0, 0],
        'yzx': [1, 0, 0],
        'zxy': [2, 0, 0]
}
EULER_EPSILON = 1e-5

def euler_to_axis_angle_yzx(alpha, beta, gamma):
    """Convert the yzx Euler angles to axis-angle notation.

    This function first generates a rotation matrix via euler_*_to_R() and then uses R_to_axis_angle() to convert to the axis and angle notation.

    @param alpha:   The alpha Euler angle in rad.
    @type alpha:    float
    @param beta:    The beta Euler angle in rad.
    @type beta:     float
    @param gamma:   The gamma Euler angle in rad.
    @type gamma:    float
    @return:        The 3D rotation axis and angle.
    @rtype:         numpy 3D rank-1 array, float
    """

    # Init.
    R = zeros((3, 3), float64)

    # Get the rotation.
    euler_to_R_yzx(alpha, beta, gamma, R)

    # Return the axis and angle.
    return R_to_axis_angle(R)
	
def euler_to_R_yzx(alpha, beta, gamma, R):
    """Generate the y-z-x Euler angle convention rotation matrix.

    Rotation matrix
    ===============

    The rotation matrix is defined as the vector of unit vectors::

        R = [mux, muy, muz].

    According to wikipedia (http://en.wikipedia.org/wiki/Euler_angles#Table_of_matrices), the rotation matrix for the yzx convention is::

              |  ca*cb              -sb                  sa*cb            |
        R  =  |  sa*sg + ca*sb*cg    cb*cg              -ca*sg + sa*sb*cg |,
              | -sa*cg + ca*sb*sg    cb*sg               ca*cg + sa*sb*sg |

    where::

        ca = cos(alpha),
        sa = sin(alpha),
        cb = cos(beta),
        sb = sin(beta),
        cg = cos(gamma),
        sg = sin(gamma).


    @param alpha:   The alpha Euler angle in rad for the y-rotation.
    @type alpha:    float
    @param beta:    The beta Euler angle in rad for the z-rotation.
    @type beta:     float
    @param gamma:   The gamma Euler angle in rad for the x-rotation.
    @type gamma:    float
    @param R:       The 3x3 rotation matrix to update.
    @type R:        3x3 numpy array
    """

    # Trig.
    sin_a = sin(alpha)
    cos_a = cos(alpha)
    sin_b = sin(beta)
    cos_b = cos(beta)
    sin_g = sin(gamma)
    cos_g = cos(gamma)

    # The unit mux vector component of the rotation matrix.
    R[0, 0] =  cos_a * cos_b
    R[1, 0] =  sin_a * sin_g  +  cos_a * sin_b * cos_g
    R[2, 0] = -sin_a * cos_g  +  cos_a * sin_b * sin_g

    # The unit muy vector component of the rotation matrix.
    R[0, 1] = -sin_b
    R[1, 1] =  cos_b * cos_g
    R[2, 1] =  cos_b * sin_g

    # The unit muz vector component of the rotation matrix.
    R[0, 2] =  sin_a * cos_b
    R[1, 2] = -cos_a * sin_g  +  sin_a * sin_b * cos_g
    R[2, 2] =  cos_a * cos_g  +  sin_a * sin_b * sin_g
	
def R_to_axis_angle(R):
    """Convert the rotation matrix into the axis-angle notation.

    Conversion equations
    ====================

    From Wikipedia (http://en.wikipedia.org/wiki/Rotation_matrix), the conversion is given by::

        x = Qzy-Qyz
        y = Qxz-Qzx
        z = Qyx-Qxy
        r = hypot(x,hypot(y,z))
        t = Qxx+Qyy+Qzz
        theta = atan2(r,t-1)

    @param R:   The 3x3 rotation matrix to update.
    @type R:    3x3 numpy array
    @return:    The 3D rotation axis and angle.
    @rtype:     numpy 3D rank-1 array, float
    """

    # Axes.
    axis = zeros(3, float64)
    axis[0] = R[2, 1] - R[1, 2]
    axis[1] = R[0, 2] - R[2, 0]
    axis[2] = R[1, 0] - R[0, 1]

    # Angle.
    r = hypot(axis[0], hypot(axis[1], axis[2]))
    t = R[0, 0] + R[1, 1] + R[2, 2]
    theta = atan2(r, t-1)

    # Normalise the axis.
    if r != 0.0:
        axis = axis / r

    # Return the data.
    return axis, theta
	
	
	
	
	
	
	
def axis_angle_to_euler_zxy(axis, angle):
    """Convert the axis-angle notation to zxy Euler angles.

    This first generates a rotation matrix via axis_angle_to_R() and then used this together with R_to_euler_zxy() to obtain the Euler angles.

    @param axis:    The 3D rotation axis.
    @type axis:     numpy array, len 3
    @param angle:   The rotation angle.
    @type angle:    float
    @return:        The alpha, beta, and gamma Euler angles in the zxy convention.
    @rtype:         float, float, float
    """

    # Init.
    R = zeros((3, 3), float64)

    # Get the rotation.
    axis_angle_to_R(axis, angle, R)

    # Return the Euler angles.
    return R_to_euler_zxy(R)
	

def axis_angle_to_R(axis, angle, R):
    """Generate the rotation matrix from the axis-angle notation.

    Conversion equations
    ====================

    From Wikipedia (http://en.wikipedia.org/wiki/Rotation_matrix), the conversion is given by::

        c = cos(angle); s = sin(angle); C = 1-c
        xs = x*s;   ys = y*s;   zs = z*s
        xC = x*C;   yC = y*C;   zC = z*C
        xyC = x*yC; yzC = y*zC; zxC = z*xC
        [ x*xC+c   xyC-zs   zxC+ys ]
        [ xyC+zs   y*yC+c   yzC-xs ]
        [ zxC-ys   yzC+xs   z*zC+c ]


    @param axis:    The 3D rotation axis.
    @type axis:     numpy array, len 3
    @param angle:   The rotation angle.
    @type angle:    float
    @param R:       The 3x3 rotation matrix to update.
    @type R:        3x3 numpy array
    """

    # Trig factors.
    ca = cos(angle)
    sa = sin(angle)
    C = 1 - ca

    # Depack the axis.
    x, y, z = axis

    # Multiplications (to remove duplicate calculations).
    xs = x*sa
    ys = y*sa
    zs = z*sa
    xC = x*C
    yC = y*C
    zC = z*C
    xyC = x*yC
    yzC = y*zC
    zxC = z*xC

    # Update the rotation matrix.
    R[0, 0] = x*xC + ca
    R[0, 1] = xyC - zs
    R[0, 2] = zxC + ys
    R[1, 0] = xyC + zs
    R[1, 1] = y*yC + ca
    R[1, 2] = yzC - xs
    R[2, 0] = zxC - ys
    R[2, 1] = yzC + xs
    R[2, 2] = z*zC + ca
	
def R_to_euler_zxy(R):
    """Convert the rotation matrix to the zxy Euler angles.

    @param R:       The 3x3 rotation matrix to extract the Euler angles from.
    @type R:        3D, rank-2 numpy array
    @return:        The alpha, beta, and gamma Euler angles in the zxy convention.
    @rtype:         tuple of float
    """

    # Redirect to R_to_euler()
    return R_to_euler(R, 'zxy')
	
def R_to_euler(R, notation, axes_rot='static', second_sol=False):
    """Convert the rotation matrix to the given Euler angles.

    This uses the algorithms of Ken Shoemake in "Euler Angle Conversion. Graphics Gems IV. Paul Heckbert (ed.). Academic Press, 1994, ISBN: 0123361567. pp. 222-229." (http://www.graphicsgems.org/).


    The Euler angle notation can be one of:
        - xyx
        - xyz
        - xzx
        - xzy
        - yxy
        - yxz
        - yzx
        - yzy
        - zxy
        - zxz
        - zyx
        - zyz


    @param R:               The 3x3 rotation matrix to extract the Euler angles from.
    @type R:                3D, rank-2 numpy array
    @param notation:        The Euler angle notation to use.
    @type notation:         str
    @keyword axes_rot:      The axes rotation - either 'static', the static axes or 'rotating', the rotating axes.
    @type axes_rot:         str
    @keyword second_sol:    Return the second solution instead (currently unused).
    @type second_sol:       bool
    @return:                The alpha, beta, and gamma Euler angles in the given convention.
    @rtype:                 tuple of float
    """

    # Duplicate R to avoid its modification.
    R = deepcopy(R)

    # Get the Euler angle info.
    i, neg, alt = EULER_TRANS_TABLE[notation]

    # Axis rotations.
    rev = 0
    if axes_rot != 'static':
        rev = 1

    # Find the other indices.
    j, k, h = matrix_indices(i, neg, alt)

    # No axis repetition.
    if alt:
        # Sine of the beta angle.
        sin_beta = sqrt(R[i, j]**2 + R[i, k]**2)

        # Non-zero sin(beta).
        if sin_beta > EULER_EPSILON:
            alpha = atan2( R[i, j],   R[i, k])
            beta  = atan2( sin_beta,  R[i, i])
            gamma = atan2( R[j, i],  -R[k, i])

        # sin(beta) is zero.
        else:
            alpha = atan2(-R[j, k],   R[j, j])
            beta  = atan2( sin_beta,  R[i, i])
            gamma = 0.0

    # Axis repetition.
    else:
        # Cosine of the beta angle.
        cos_beta = sqrt(R[i, i]**2 + R[j, i]**2)

        # Non-zero cos(beta).
        if cos_beta > EULER_EPSILON:
            alpha = atan2( R[k, j],   R[k, k])
            beta  = atan2(-R[k, i],   cos_beta)
            gamma = atan2( R[j, i],   R[i, i])

        # cos(beta) is zero.
        else:
            alpha = atan2(-R[j, k],  R[j, j])
            beta  = atan2(-R[k, i],   cos_beta)
            gamma = 0.0

    # Remapping.
    if neg:
        alpha, beta, gamma = -alpha, -beta, -gamma
    if rev:
        alpha_old = alpha
        alpha = gamma
        gamma = alpha_old

    # Angle wrapping.
    if alt and -pi < beta < 0.0:
        alpha = alpha + pi
        beta = -beta
        gamma = gamma + pi

    alpha = wrap_angles(alpha, 0.0, 2.0*pi)
    beta  = wrap_angles(beta,  0.0, 2.0*pi)
    gamma = wrap_angles(gamma, 0.0, 2.0*pi)

    # Return the Euler angles.
    return alpha, beta, gamma
	
def wrap_angles(angle, lower, upper, window=2*pi):
    """Convert the given angle to be between the lower and upper values.

    @param angle:   The starting angle.
    @type angle:    float
    @param lower:   The lower bound.
    @type lower:    float
    @param upper:   The upper bound.
    @type upper:    float
    @param window:  The size of the window where symmetry exists (defaults to 2pi).
    @type window:   float
    @return:        The wrapped angle.
    @rtype:         float
    """

    # Check the bounds and window.
    if window - (upper - lower) > 1e-7:
        raise RelaxError

    # Keep wrapping until the angle is within the limits.
    while True:
        # The angle is too big.
        if angle > upper:
            angle = angle - window

        # The angle is too small.
        elif angle < lower:
            angle = angle + window

        # Inside the window, so stop wrapping.
        else:
            break

    # Return the wrapped angle.
    return angle
	
def matrix_indices(i, neg, alt):
    """Calculate the parameteric indices i, j, k, and h.

    This is one of the algorithms of Ken Shoemake in "Euler Angle Conversion. Graphics Gems IV. Paul Heckbert (ed.). Academic Press, 1994, ISBN: 0123361567. pp. 222-229."  (http://www.graphicsgems.org/).

    The indices (i, j, k) are a permutation of (x, y, z), and the index h corresponds to the row containing the Givens argument a.


    @param i:   The index i.
    @type i:    int
    @param neg: Zero if (i, j, k) is an even permutation of (x, y, z) or one if odd.
    @type neg:  int
    @param alt: Zero if the first and last system axes are the same, or one if they are different.
    @type alt:  int
    @return:    The values of j, k, and h.
    @rtype:     tuple of int
    """

    # Calculate the indices.
    j = EULER_NEXT[i + neg]
    k = EULER_NEXT[i+1 - neg]

    # The Givens rotation row index.
    if alt:
        h = k
    else:
        h = i

    # Return.
    return j, k, h
	
	
	
	
	
	
def R_to_euler_zxy(R):
    """Convert the rotation matrix to the zxy Euler angles.

    @param R:       The 3x3 rotation matrix to extract the Euler angles from.
    @type R:        3D, rank-2 numpy array
    @return:        The alpha, beta, and gamma Euler angles in the zxy convention.
    @rtype:         tuple of float
    """

    # Redirect to R_to_euler()
    return R_to_euler(R, 'zxy')
	
def R_to_euler(R, notation, axes_rot='static', second_sol=False):
    """Convert the rotation matrix to the given Euler angles.

    This uses the algorithms of Ken Shoemake in "Euler Angle Conversion. Graphics Gems IV. Paul Heckbert (ed.). Academic Press, 1994, ISBN: 0123361567. pp. 222-229." (http://www.graphicsgems.org/).


    The Euler angle notation can be one of:
        - xyx
        - xyz
        - xzx
        - xzy
        - yxy
        - yxz
        - yzx
        - yzy
        - zxy
        - zxz
        - zyx
        - zyz


    @param R:               The 3x3 rotation matrix to extract the Euler angles from.
    @type R:                3D, rank-2 numpy array
    @param notation:        The Euler angle notation to use.
    @type notation:         str
    @keyword axes_rot:      The axes rotation - either 'static', the static axes or 'rotating', the rotating axes.
    @type axes_rot:         str
    @keyword second_sol:    Return the second solution instead (currently unused).
    @type second_sol:       bool
    @return:                The alpha, beta, and gamma Euler angles in the given convention.
    @rtype:                 tuple of float
    """

    # Duplicate R to avoid its modification.
    R = deepcopy(R)

    # Get the Euler angle info.
    i, neg, alt = EULER_TRANS_TABLE[notation]

    # Axis rotations.
    rev = 0
    if axes_rot != 'static':
        rev = 1

    # Find the other indices.
    j, k, h = matrix_indices(i, neg, alt)

    # No axis repetition.
    if alt:
        # Sine of the beta angle.
        sin_beta = sqrt(R[i, j]**2 + R[i, k]**2)

        # Non-zero sin(beta).
        if sin_beta > EULER_EPSILON:
            alpha = atan2( R[i, j],   R[i, k])
            beta  = atan2( sin_beta,  R[i, i])
            gamma = atan2( R[j, i],  -R[k, i])

        # sin(beta) is zero.
        else:
            alpha = atan2(-R[j, k],   R[j, j])
            beta  = atan2( sin_beta,  R[i, i])
            gamma = 0.0

    # Axis repetition.
    else:
        # Cosine of the beta angle.
        cos_beta = sqrt(R[i, i]**2 + R[j, i]**2)

        # Non-zero cos(beta).
        if cos_beta > EULER_EPSILON:
            alpha = atan2( R[k, j],   R[k, k])
            beta  = atan2(-R[k, i],   cos_beta)
            gamma = atan2( R[j, i],   R[i, i])

        # cos(beta) is zero.
        else:
            alpha = atan2(-R[j, k],  R[j, j])
            beta  = atan2(-R[k, i],   cos_beta)
            gamma = 0.0

    # Remapping.
    if neg:
        alpha, beta, gamma = -alpha, -beta, -gamma
    if rev:
        alpha_old = alpha
        alpha = gamma
        gamma = alpha_old

    # Angle wrapping.
    if alt and -pi < beta < 0.0:
        alpha = alpha + pi
        beta = -beta
        gamma = gamma + pi

    alpha = wrap_angles(alpha, 0.0, 2.0*pi)
    beta  = wrap_angles(beta,  0.0, 2.0*pi)
    gamma = wrap_angles(gamma, 0.0, 2.0*pi)

    # Return the Euler angles.
    return alpha, beta, gamma
	
	
	
	
	
	
	
	
axis, angle = euler_to_axis_angle_yzx(radians(eulerAngles.EULERY), radians(eulerAngles.EULERZ), radians(eulerAngles.EULERX))
print("Axis is %s" % axis)
print("angle is %s" % angle)
alpha, beta, gamma = axis_angle_to_euler_zxy(axis, angle)
print("alpha (Z) is %s" % degrees(alpha))
print("beta (X) is %s" % degrees(beta))
print("gamma (Y) is %s" % degrees(gamma))