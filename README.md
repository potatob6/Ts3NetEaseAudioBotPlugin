# TS3AudioBot-NetEaseCloudmusic-UNM
TS3AudioBot-NetEaseCloudmusic-UnblockNeteaseMusic-plugin

支持Windows、Linux、Docker环境。

## 关于解锁版权歌曲
需要在自建的 [NeteaseCloudMusicApi](https://github.com/Binaryify/NeteaseCloudMusicApi)（推荐Docker版）里面的 app.js 中添加 `process.env['NODE_TLS_REJECT_UNAUTHORIZED'] = 0`, 如果是docker版的话就在环境里面添加`NODE_TLS_REJECT_UNAUTHORIZED = 0`。  
需要自建 [UnblockNeteaseMusic](https://github.com/UnblockNeteaseMusic/server) 服务（推荐Docker版）。

## 关于设置文件YunSettings.ini
`playMode=`是播放模式   
`WangYiYunAPI_Address=`是网易云API地址   
`cookies1=`是保存在你本地的身份验证，通过二维码登录获取。（不需要修改）   
`UNM_Address=`是 UnblockNeteaseMusic 服务的API地址。    

## 替换插件文件后需要重启TS3AudioBot服务！！！

## 目前的指令：
正在播放的歌曲的图片和名称可以点机器人看它的头像和描述  
vip音乐想要先登陆才能播放完整版本:（输入指令后扫描机器人头像二维码登陆）
`!yun login`  

双击机器人，目前有以下指令
1.立即播放网易云音乐  
`!yun play 音乐名称` 或 `!yun play 音乐名称 歌手` (无版权歌曲点播)  
  
2.播放网易云音乐歌单    
`!yun gedan 歌单id`  

3.播放列表中的下一首    
`!yun next`  

3.停止播放    
`!yun stop` 

5.修改播放模式    
`!yun mode 数字0-3`    
`0 = 顺序播放`    
`1 = 顺序循环`    
`2 = 随机播放`    
`3 = 随机循环`    

# 关于docker TS3AudioBot自行构建 bot来自主线[TS3AudioBot](https://github.com/Splamy/TS3AudioBot) 
## 发现的问题：在armbian中运行容器出现关于ts3audiobot.db权限问题导致无法使用，youtube-dl版本过低
解决方案：取消Dockerfile原有的子账户运行，改为root权限运行，改用yt-dlp（需自行修改ts3audiobot.toml中youtube-dl = { path = "yt-dlp" }）

关于自行构建系统架构问题，已经在Dockerfile安排了3套架构x86、arm64、arm32默认X86，需要哪种用哪种。
`docker build -f Dockerfile -t local.docker.image/ts3audiobot:latest .` 
运行方法同样参考[TS3AudioBot_docker](https://github.com/getdrunkonmovies-com/TS3AudioBot_docker) 


## 使用的开源库

[Nini](https://github.com/bmatzelle/nini)     
[Costura.Fody](https://github.com/Fody/Costura/)  
[TS3AudioBot](https://github.com/Splamy/TS3AudioBot)   
[ZHANGTIANYAO1/TS3AudioBot-NetEaseCloudmusic-plugin](https://github.com/ZHANGTIANYAO1/TS3AudioBot-NetEaseCloudmusic-plugin) 
