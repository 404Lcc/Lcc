# Lcc

Lcc框架

优点：高扩展，低耦合，快速开发

## V1.0

**简介**

[Lcc](https://github.com/404Lcc/Lcc)是针对Unity开发的轻量级框架，可快速上手开发**Steam**、**安卓**、**IOS**等项目

**特点**

1. 适配ILRuntime，主工程集与热更程集不继承MonoBehaviour而继承ObjectBase实现生命周期，并对原有的生命周期进行了拓展

2. 场景里面的所有脚本都通过LccView来管理，通过LccView工厂可快速获取到ObjectBase

3. 继承ObjectBase编写代码避免了一些坑，而且可以无缝切换

4. 提供管理器自动化流程，减少代码量

5. UI部分提供MVVM，继承即可使用

4. 提供编辑器工具帮助快速配置项目出包

5. 提供主工程集与热更程集代码模板，无需编写样板代码

6. 提供ObjectBase可视化工具，帮助快速开发，解决bug

7. 提供ObjectBase对象里字段的自动化查找，无需手动编写查找代码或拖拽，减少代码量

8. 定义模块事件，编写Handler即可处理，需传开启新模块所需数据，达到解耦效果，结构清晰

9. 定义UI事件，编写Handler标记特性即可处理

10. 代码全自动加密

11. 资源异步加载

**热更新**

1. Lua方案[XLua](https://github.com/Tencent/xLua)

2. C#方案[ILRuntime](https://github.com/Ourpalm/ILRuntime)

3. 资源热更新Addressable

**ILRuntime热更新注意项**

1. 组件缓存

2. for代替foreach

3. 适配器可通过编辑器工具生成基础模板，大部分时候不用自己编写

4. 打包之前一定要生成CLR绑定代码，否则会被IL2CPP裁剪

## 主要功能

**UI管理**

UI容器，UI管理，UI工具等

**场景管理**

**音频管理**

**多文本管理**

**AStar寻路**

**数据加密**

**事件系统**

**命令系统**

**网络系统等**

以上都是通过LccView对ObjectBase进行管理

## 快速开始

**UI**

支持MVVM，提供了视图、视图模型、数据绑定

``` csharp
PanelManager.Instance.OpenPanel(PanelType.Launch);
```

**场景加载**

提供加载ab包场景或者加载本地场景

``` csharp
LoadSceneManager.Instance.LoadScene(SceneName.Login, null, AssetType.Scene);
```

## 开发环境

- Unity2020

## 贡献成员

- [404Lccxy](https://github.com/404Lccxy)

## 项目

- [个人独立游戏](https://www.taptap.com/developer/6782)
