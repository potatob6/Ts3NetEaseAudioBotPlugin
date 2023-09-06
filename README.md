# TS3AudioBot-NetEaseCloudmusic-plugin
在ZHANGTIANYAO1/TS3AudioBot-NetEaseCloudmusic-plugin的基础上兼容Linux,并且支持版权歌曲解锁（还未实现）。
使用 Nini .NET配置库（http://nini.sourceforge.net/）实现ini文件的操作。
使用 Costura.Fody 打包。

## 关于设置文件YunSettings.ini
`playMode=`是播放模式   
`WangYiYunAPI_Address=`是网易云 API 地址，目前默认的是一个大佬的远程 API，如果加载速度过慢或者无法访问，请自行部署 API 并修改 API 地址。（为了保护你的隐私强烈建议你自行部署 API ）   
`cookies1=`是保存在你本地的身份验证，通过二维码登录获取。（不需要修改）   
`UNM_Address=`是 UnblockNeteaseMusic 服务的 API 地址。（还未实现）

## 目前的指令：
正在播放的歌单的图片和名称可以点机器人看它的头像和描述  
vip音乐想要先登陆才能播放完整版本:（输入指令后扫描机器人头像二维码登陆）
`!yun login`  

双击机器人，目前有以下指令（把[xxx]替换成对应信息，**包括中括号**）
1.立即播放网易云音乐  
`!yun play [音乐名称]`  
  
2.添加音乐到下一首  
`!yun add [音乐名称]`  
  
3.播放网易云音乐歌单(如果提示Error: Nothing to play...重新输入指令解决)  
`!yun gedan [歌单名称]`  
  
4.播放网易云音乐歌单id  
`!yun gedanid [歌单名称]`  
  
5.立即播放网易云音乐id  
`!yun playid [歌单id]`  
  
6.添加指定音乐id到下一首  
`!yun add [音乐id]`  
  
7.播放列表中的下一首    
`!yun next`  

8.修改播放模式    
`!yun mode [模式选择数字0-3]`  
`0 = 顺序播放`
`1 = 顺序循环`
`2 = 随机播放`
`3 = 随机循环`

需要注意的是如果歌单歌曲过多需要时间加载，期间一定一定不要输入其他指令  
