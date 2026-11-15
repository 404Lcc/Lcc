# Lcc

[Lcc](https://github.com/404Lcc/Lcc) 是一个 Unity 游戏开发框架，围绕模块化服务组织游戏代码，将资源与代码热更新、界面与流程管理、局内战斗串联起来。仓库包含通用框架和当前项目的游戏业务，可用于阅读完整接入方式，也可按层复用。

## 核心特点

- **模块化组织**：通过 `Main` / `Module` 安装服务、管理生命周期，在 `GameMain` 中组装当前项目所需的模块与实现。
- **资源与代码热更新**：YooAsset 负责资源加载、版本清单和下载，HybridCLR 负责 Player 中的热更程序集加载与 AOT 元数据补充。
- **分层战斗框架**：基础能力、通用战斗内核与项目玩法分层，技能、Buff 和子物体通过逻辑配置与节点组合。
- **常用开发能力**：提供 UI/MVVM、本地化、音频、对象池、配置、存档与引导，并包含网络、Steam 房间接入及编辑器工具。

## 架构概览

```text
Assets/Framework          主工程：启动、配置与通用工具
Assets/Hotfix/Framework   热更框架：模块服务、UI、实体与节点逻辑
Assets/Hotfix/GameLogic   项目业务：模块组装、流程、界面与玩法
Assets/Editor             编辑器工具
```

战斗部分位于 `GameLogic/CoreGame`，依赖方向为 `BattleGameplay → BattleKernel → BattleFoundation`。通用内核定义扩展接口，玩法层注入具体实现。完整目录与源码入口见 [架构说明](docs/架构说明.md)。

## 快速开始

1. 使用 Unity **2023.2.20f1** 打开仓库根目录，等待依赖恢复与脚本编译。
2. 按 [快速开始](docs/快速开始.md) 准备本地运行配置并生成启动配置。
3. 打开 `Assets/Scenes/Launch.unity`，进入 Play Mode。

## 文档导航

| 文档 | 内容 |
| --- | --- |
| [快速开始](docs/快速开始.md) | 环境、配置与首次启动 |
| [架构说明](docs/架构说明.md) | 工程目录、模块职责、启动链路与战斗分层 |
| [热更新与构建](docs/热更新与构建.md) | 资源运行模式、程序集加载与构建前提 |
| [战斗系统与 DSL](docs/战斗系统与DSL.md) | 战斗分篇导读、DSL 与施法到清理案例 |
| [为什么使用 DSL 与 Entity](docs/battle/为什么使用DSL与Entity.md) | 技能复用、统一驱动与维护收益，附单发改三连发示例 |
| [开发工具](docs/开发工具.md) | 数据表、协议、本地化与调试工具 |

## 依赖与许可

主要依赖包括 [HybridCLR](https://github.com/focus-creative-games/hybridclr)、[YooAsset](https://github.com/tuyoogame/YooAsset)、[Luban](https://github.com/focus-creative-games/luban)，以及 Mirror、FishNet、Steamworks.NET 等。包依赖见 [manifest.json](Packages/manifest.json)，随工程放置的插件见 [Assets/3rdPlugins](Assets/3rdPlugins)。

Lcc 框架采用 [MIT License](LICENSE)。第三方插件遵循各自许可，Odin Inspector 等商业插件需按其授权条款使用。

## 交流与作者

- 项目地址：[404Lcc/Lcc](https://github.com/404Lcc/Lcc)
- Lcc 框架交流群：[679054092](https://jq.qq.com/?_wv=1027&k=KPEIZeok)
- 作者：[404Lccxy](https://github.com/404Lccxy)
- [个人项目](https://www.taptap.com/developer/6782)
