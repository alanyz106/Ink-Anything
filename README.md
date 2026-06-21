<div align="center">

<img src="Ink%20Anything/Resources/Ink%20Anything.png?raw=true" alt="LOGO" width="256" height="256"/>

# Ink-Anything

基于 WPF/C# 的轻量级数字画板，支持文本输入，针对希沃白板和 PowerPoint 放映深度优化。

</div>

[English](README_EN.md) | 中文

## 特性

- 对 Microsoft PowerPoint 有优化支持，放映时自动切换画板模式，支持翻页控制和墨迹/文本保存
- 支持 Active Pen（压感支持）
- 笔细的一头写字，反过来粗的一头是橡皮擦（希沃白板本身不支持此功能）
- 手指直接触摸擦除墨迹
- 多点触控手势支持（缩放、平移、旋转）
- 白板多页管理，支持无限画布
- 手绘图形识别（圆、矩形、三角形等），自动转换为规范图形
- **文本输入**：支持在画布任意位置添加文字，可拖拽移动、缩放大小、更改颜色，支持撤销/重做，白板翻页和 PPT 放映中自动保存；支持 **Ctrl+点击多选** 文本元素，**Ctrl+拖动复制**所有选中的文字
- **橡皮擦三态切换**：笔画擦除 ↔ 部分擦除 ↔ 退出橡皮擦，支持擦除文本元素
- **墨迹选区增强**：支持选中墨迹后拖动移动，Ctrl+拖动复制，框选外部点击自动取消选中；选择按钮支持三态切换（进入选择 → 全选 → 退出）
- 浮动工具栏，支持锁定/解锁位置
- 倒计时器、随机点名等教学辅助工具
- 截图并自动保存
- 墨迹保存与加载
- 墨迹回放功能
- 深色/浅色/跟随系统主题切换
- 开机自启动
- 全局热键支持（Alt+S/D/E/C/V/L/T/Q，Alt+1~6 快速切换画笔颜色，Ctrl+A 全选）
- 启动时自动检测快捷键冲突并提示
- 对其他红外触控屏也可提供相似功能

## 键盘快捷键

| 快捷键 | 功能 |
|---|---|
| Alt+S | 切换画笔/鼠标模式 |
| Alt+D | 清屏 |
| Alt+E | 橡皮擦循环切换（笔画擦 → 部分擦 → 退出） |
| Alt+C | 截屏 |
| Alt+V | 显示/隐藏浮动工具栏 |
| Alt+L | 画直线 |
| Alt+T | 文本输入模式 |
| Alt+Q | 切换选择模式 |
| Alt+1~6 | 切换画笔颜色（黑/红/绿/蓝/黄/白） |
| Ctrl+A | 全选墨迹 |
| Ctrl+Z | 撤销 |
| Ctrl+Y | 恢复 |
| Shift+Esc | 退出/结束放映 |
| Escape | 退出当前模式 |

## 运行环境

- Windows 10 及以上
- .NET Framework 4.7.2
- Microsoft Office（PPT 集成需要）

## 用户手册

详细的使用说明请参阅 [用户手册](Manual.md)。

## 鸣谢

本项目基于 [Ink Canvas](https://github.com/WXRIW/Ink-Canvas) 开发，感谢原项目作者及所有贡献者的辛勤付出。

## 赞赏

如果觉得这个项目对你有帮助，欢迎请作者喝杯咖啡~

<img src="Ink%20Anything/Resources/alipay.jpg?raw=true" alt="支付宝赞赏码" width="256"/>

<img src="Ink%20Anything/Resources/wxpay.png?raw=true" alt="微信赞赏码" width="256"/>

## License

GPL-3.0 License
