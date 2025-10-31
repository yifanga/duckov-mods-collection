### 原始说明版本

本mod实现了 钥匙录入，工作台蓝图录入，医疗站蓝图录入，矿机取放显卡 物品维修，物品分解 界面的物品过滤优化

---------------------

在游戏中钥匙和蓝图录入时，左侧背包和仓库会展示所有的物品，十分不方便。

本mod可以在钥匙和蓝图录入时自动过滤其他物品，只保留可以录入的钥匙和蓝图。

注意本mod依赖HarmonyLib，需要同时订阅HarmonyLib，并且需要将【HarmonyLib】移动至最上方优先启用

---------------------

2025-10-28 更新
1. 支持了矿机的显卡过滤，会自动过滤仓库内的物品，只展示显卡和比特币，背包不过滤方便取出比特币
2. 完善了医疗配方录入的支持，目前医疗配方应当也能正常保留
2025-10-31 更新
1. 支持了物品维修界面的过滤，只会展示背包中可维修的物品
2. 支持了物品分解页面仓库物品的过滤，只会展示可分解物品，背包不过滤方便继续处理分解产物

---------------------

When registering keys and blueprints, the left-side backpack and storage display all items, which is highly inconvenient.
This mod automatically filters out other items during key and blueprint registration, retaining only registerable keys and blueprints.

---------------------
Update 2025-10-28

Added GPU filtering for mining rigs: Automatically filters storage items to display only GPUs and Bitcoins. Backpack remains unfiltered for easy Bitcoin retrieval.
Enhanced support for Medical Formula registration. Medical Formulas should now be properly supported.


### 说明更新为ai辅助生成的版本
```
[h2][b]🔍 智能物品过滤优化[/b][/h2]
[b]✅ 功能说明：[/b]

优化游戏内多个交互界面的物品显示，自动过滤无效物品，提升操作效率：

• 钥匙录入：仅显示可录入的钥匙
• 蓝图录入：仅显示工作台/医疗站可用蓝图
• 矿机管理：仓库仅显示显卡和比特币（背包不过滤，方便取出比特币）
• 物品维修：仅显示可维修物品
• 物品分解：仓库仅显示可分解物品（背包不过滤，方便操作分解产物）

[b]🔄 更新日志：[/b]
[list][*] [b]2025-10-31[/b]
    - 新增物品维修界面过滤
    - 新增物品分解界面过滤
[*] [b]2025-10-28[/b]
    - 支持矿机显卡和比特币过滤
    - 完善医疗配方支持[/list]
[b]⚠️ 重要提示：[/b]
1. 必须同时订阅 [b]HarmonyLib[/b]
2. 在模组列表中需将 [b]HarmonyLib[/b] 移至最上方优先启用

[h2][b]🔍 Smart Item Filter[/b][/h2]
[b]✅ Features:[/b]

Optimizes item display in multiple interfaces by automatically filtering irrelevant items:

• Key Registration: Shows only registerable keys
• Blueprint Registration: Shows only workbench/medical station blueprints
• Mining Rig: Storage shows only GPUs and Bitcoins (backpack unfiltered)
• Item Repair: Shows only repairable items
• Item Decomposition: Storage shows only decomposable items (backpack unfiltered)
[b]🔄 Changelog:[/b]
[list][*] [b]2025-10-31[/b]
    - Added item repair interface filter
    - Added item decomposition interface filter
[*] [b]2025-10-28[/b]
    - Added GPU and Bitcoin filtering for mining rigs
    - Enhanced medical formula support[/list]
[b]⚠️ Important:[/b]
1. Requires [b]HarmonyLib[/b] subscription
2. Must place [b]HarmonyLib[/b] at top of mod list
```