# Unity3D角色动画系统

#### 介绍
Unity3D角色动画系统

#### 软件架构
软件架构说明

#### 参考教程
- B站up主IGBeginner0116：[Unity入门教程，初学者请按顺序观看](https://space.bilibili.com/269749034/channel/collectiondetail?sid=48663&spm_id_from=333.788.0.0)

#### 第三方资源
- 模型：原神-妮露
- 动作：
    - [Mixamo](https://www.mixamo.com/)
    - [EveryDayMotionPack](https://assetstore.unity.com/packages/3d/animations/everyday-motion-pack-116611)
- 音效：
    - B站[【首发素材】《崩坏3》DLC《后崩坏书》角色语音素材库全网首发！可供下载使用。（持续更新中）](https://www.bilibili.com/video/BV18E411M7uZ/?p=5&spm_id_from=333.880.my_history.page.click)
- Shader：
    - GitHub [SimpleURPToonLitExample](https://github.com/ColinLeung-NiloCat/UnityURPToonLitShaderExample?tab=MIT-1-ov-file)

#### 开发日记
##### 2024.12.9-2024.12.15
###### 新增功能
- 新建了项目文件夹:)
- 基本完成了基础动画和状态机的调整（后续可能会持续优化）
- 手写了基础的代码控制地面检测
- 调整了第三人称摄像机的基础参数
- 完成了基础的移动、跳跃、下蹲等动作的代码控制
- 新增了两个表情动画“问好”和“赞同”
- 为脚步声和跳跃添加了基础的音效（脚步声音效后期应该还会修改）
###### 待修复Bug
- 角色在贴墙时跳跃会发生异常的下蹲动作切换，且该切换疑似会打断跳跃动作
- 角色在上坡时跳跃会导致播放两次跳跃音效（应该是因为上坡时跳跃的地面检测造成的）

#### 特技
1.  使用 Readme\_XXX.md 来支持不同的语言，例如 Readme\_en.md, Readme\_zh.md
2.  Gitee 官方博客 [blog.gitee.com](https://blog.gitee.com)
3.  你可以 [https://gitee.com/explore](https://gitee.com/explore) 这个地址来了解 Gitee 上的优秀开源项目
4.  [GVP](https://gitee.com/gvp) 全称是 Gitee 最有价值开源项目，是综合评定出的优秀开源项目
5.  Gitee 官方提供的使用手册 [https://gitee.com/help](https://gitee.com/help)
6.  Gitee 封面人物是一档用来展示 Gitee 会员风采的栏目 [https://gitee.com/gitee-stars/](https://gitee.com/gitee-stars/)
