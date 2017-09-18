@ECHO off
REM example client up script for windows
REM will be executed when client is up
 
REM all key value pairs in ShadowVPN config file will be passed to this script
REM as environment variables, except password
 
REM user-defined variables
REM SET remote_tun_ip=10.7.0.0
      ::#server virtual net IP
SET dns_server=8.8.8.8
          ::#set dns
REM SET orig_intf="WLAN 2"
            ::#actual interface to connect to network
 
REM exclude remote server in routing table
for /F "tokens=3" %%* in ('route print ^| findstr "\<0.0.0.0\>"') do set "orig_gw=%%*"
route change 0.0.0.0/0 %orig_gw%
route add %server% %orig_gw%

REM configure IP address and MTU of VPN interface
REM netsh interface ip set interface %orig_intf% ignoredefaultroutes=enabled > NUL
netsh interface ip set address name="%intf%" static %tunip% 255.255.255.0 > NUL
REM netsh interface ipv4 set address name="%intf%" static %tunip% 255.255.255.0 %remote_tun_ip% gwmetric=10000 > NUL
REM netsh interface ip set interface "%intf%" metric=1 > NUL
netsh interface ipv4 set subinterface "%intf%" mtu=%mtu% > NUL
 
REM change routing table
ECHO changing default route
REM checking if winxp
REM ver | find "5.1" > NUL
REM if %ERRORLEVEL%==0 (
    route add 128.0.0.0 mask 128.0.0.0 %remote_tun_ip% metric 1 IF %intf_id% > NUL
    route add 0.0.0.0 mask 128.0.0.0 %remote_tun_ip% metric 1  IF %intf_id% > NUL
REM ) else (
REM     netsh interface ipv4 add route 128.0.0.0/1 "%intf%" %remote_tun_ip% metric=0 > NUL
REM     netsh interface ipv4 add route 0.0.0.0/1 "%intf%" %remote_tun_ip% metric=0 > NUL
REM )
ECHO default route changed to %remote_tun_ip%
 
REM change dns server
netsh interface ip set dns name="%intf%" static %dns_server% > NUL
netsh interface ip set dns name="%orig_intf_id%" static %dns_server% > NUL
 
ECHO %0 done