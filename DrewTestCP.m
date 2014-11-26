function DrewTestCP

 run('C:\Projects\OpenCvSharpTest\UnityProject\pointData.m')
 
 %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
 
 
 kc = [0.0 0.0 0.0 0.0 0.0 0.0 0.0 0.0];
alpha_c = 0.0;

objpointsinv = [objectPoints(1,:) ; objectPoints(2,:) ; objectPoints(3,:)  * -1 ];

[omckk,Tckk,Rckk,H,x,ex] = compute_extrinsic(imagePoints,objpointsinv,fc,cc,kc,alpha_c);

% flip back the axis we flipped to get RHCS
Rckk(1:3,3) = Rckk(1:3,3) * -1;

((Rckk' * -1) * Tckk)

x = 12;

% True translation of camera:
% x: -2.378186 y: 1 z: -9.490389
