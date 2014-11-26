
%[omckk,Tckk,Rckk,H,x,ex] = compute_extrinsic(x_kk,X_kk,fc,cc,kc,alpha_c)
%
%Computes the extrinsic parameters attached to a 3D structure X_kk given its projection
%on the image plane x_kk and the intrinsic camera parameters fc, cc and kc.
%Works with planar and non-planar structures.
%
%INPUT: x_kk: Feature locations on the images
%       X_kk: Corresponding grid coordinates
%       fc: Camera focal length
%       cc: Principal point coordinates
%       kc: Distortion coefficients
%       alpha_c: Skew coefficient
%OUTPUT: omckk: 3D rotation vector attached to the grid positions in space
%        Tckk: 3D translation vector attached to the grid positions in space
%        Rckk: 3D rotation matrices corresponding to the omc vectors
%        H: Homography between points on the grid and points on the image plane (in pixel)
%           This makes sense only if the planar that is used in planar.
%        x: Reprojections of the points on the image plane
%        ex: Reprojection error: ex = x_kk - x;

imagePoints = [256, 12;
242, 341;
749, 362;
678, 196;
595, 206;
512, 220;
371, 203;
506, 110;
456, 14;
254, 149;]';

objectPoints = [-7.1087265014648, 8.2005195617675, 6.13520765304565;
-7.6523532867431, -5.568284034729, 6.39236736297607;
8.67125129699707, -4.531543731689, -2.6654295921325;
14.5806922912598, 0.3208838105201, 4.76360177993774;
11.7524862289429, -0.457778573036, 8.64883232116699;
1.29042196273804, -0.004646010696, -1.1793675422668;
-2.4835014343261, 0.5344604849815, -0.9822862744331;
3.66202616691589, 4.6307878494262, 4.91419506072998;
1.15246772766113, 8.8704557418823, 4.88052654266357;
-22.152841567993, 7.0923838615417, 54.8600120544434;]';

fc = [319.56338500976562, 319.56338500976562];
cc = [481.5, 184.5];
kc = [0.0 0.0 0.0 0.0 0.0 0.0 0.0 0.0];
alpha_c = 0.0;

objpointsinv = [objectPoints(1,:) ; objectPoints(2,:) ; objectPoints(3,:)  * -1 ];

[omckk,Tckk,Rckk,H,x,ex] = compute_extrinsic(imagePoints,objpointsinv,fc,cc,kc,alpha_c);

% flip back the axis we flipped to get RHCS
% Rckk(1:3,3) = Rckk(1:3,3) * -1

((Rckk' * -1) * Tckk)


% True translation of camera:
% x: -2.378186 y: 1 z: -9.490389


