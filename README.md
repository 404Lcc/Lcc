# Lcc
Lcc框架

## V1.0

**简介**

[Lcc](https://github.com/404Lcc/Lcc)是针对Unity开发的轻量级框架，可快速上手开发**Steam**、**安卓**、**IOS**等项目。

**热更新**

1. Lua方案[XLua](https://github.com/Tencent/xLua)

2. C#方案[ILRuntime](https://github.com/Ourpalm/ILRuntime)

3. 资源热更新[XAsset](https://github.com/xasset/xasset)

4. C#代码加密

**ILRuntime热更新注意项**

1. 组件缓存

2. Vector尽量不用.x .y .z

3. for代替foreach

4. 适配器可通过编辑器工具生成基础模板，大部分时候不用自己编写

5. 打包之前一定要生成CLR绑定代码，否则会被IL2CPP裁剪

## 主要功能

**UI管理**

**场景管理**

**音频管理**

**多文本管理**

**AStar寻路**

**数据加密**

**事件系统**

**命令系统**

**TCP、Http等协议**

## 快速开始

**UI**

支持MVVM，提供了视图、视图模型、数据绑定。

``` csharp
PanelManager.Instance.OpenPanel(PanelType.Launch);
```

**场景加载**

``` csharp
LoadSceneManager.Instance.LoadScene(SceneName.Login, null, AssetType.Scene);
```

## 开发环境

- Unity2020

## 贡献成员

- [404Lccxy](https://github.com/404Lccxy)

## 项目

- [幻世激战](https://www.taptap.com/app/20877)
