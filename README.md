# TS3AudioBot-NetEaseCloudmusic-UNM
TS3AudioBot-NetEaseCloudmusic-UnblockNeteaseMusic-plugin


## 关于解锁版权歌曲
需要在自建的 [NeteaseCloudMusicApi](https://github.com/Binaryify/NeteaseCloudMusicApi)（推荐Docker版）里面的 app.js 中添加 `process.env['NODE_TLS_REJECT_UNAUTHORIZED'] = 0`, 如果是docker版的话就在环境里面添加`NODE_TLS_REJECT_UNAUTHORIZED = 0`。  
需要自建 [UnblockNeteaseMusic](https://github.com/UnblockNeteaseMusic/server) 服务（推荐Docker版）。

## 关于设置文件YunSettings.ini
`playMode=`是播放模式   
`WangYiYunAPI_Address=`是网易云API地址，目前默认的是一个大佬的远程 API，如果加载速度过慢或者无法访问，请自行部署API并修改API地址。（为了保护你的隐私强烈建议你自行部署API）   
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

4.修改播放模式    
`!yun mode 数字0-3`    
`0 = 顺序播放`    
`1 = 顺序循环`    
`2 = 随机播放`    
`3 = 随机循环`    


## 使用的开源库

[Nini](https://github.com/bmatzelle/nini)     
[Costura.Fody](https://github.com/Fody/Costura/)    
[ZHANGTIANYAO1/TS3AudioBot-NetEaseCloudmusic-plugin](https://github.com/ZHANGTIANYAO1/TS3AudioBot-NetEaseCloudmusic-plugin) 