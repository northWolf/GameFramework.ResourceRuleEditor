# GameFramework.AssetBundleRuleEditor

#### 介绍
This is a Unity assetbundle rule editor visual tool based on GameFramework. 这是一个基于GameFramework框架的AB规则编辑器,支持按自定义规则自动生成AssetBundleCollection.xml,
省却手动配置的麻烦。
#### 使用说明

1.  假设你本地已有 GameFramework 或者 StarForce 项目。
2.  Clone本仓库,拷贝Assets/GameMain/Scripts/Editor/AssetBundleRuleEditor 文件夹到项目Editor文件夹中,等待编译完成。
3.  打开菜单 GameFramework/AssetBundle Tools/AssetBundle Rule Editor,出现如图所示的窗口。
![](imgs\Editor.png)
4.  点击 Add 或 + ,添加一条规则记录, 指定一个文件夹,并分配 过滤类型(FilterType) 和 通配符(Patterns) ,支持可选参数: 资源组列表(Groups),
AB变体(Varient),打进包内(Packed)。
5.  过滤类型(FilterType)说明: 
		Root 是指定文件夹打成一个AB。
		Children 指定文件夹下的文件分别打成一个AB。
		Children Folders Only 指定文件夹下的子文件夹分别打成一个AB。
		Children Files Only 指定文件夹下的子文件夹的文件分别打成一个AB。
6.  Groups,Pattens 多个参数值可以用 "," , ";" , "|" 来分割。
7.  AB名称(AssetBundleName)可缺省。
		当过滤类型选择为 Root 或 Children Folders Only, AB名称会自动命名为文件夹的名称。
		当过滤类型选择为 Children 或 Children Files Only,AB名称会自动命名为文件的名称。