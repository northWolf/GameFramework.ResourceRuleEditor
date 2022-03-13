# GameFramework.ResourceRuleEditor

#### 介绍
This is a Unity resource(AssetBundle) rule editor visual tool based on GameFramework. 这是一个基于GameFramework框架的资源包（AssetBundle）规则编辑器,支持按自定义规则自动生成ResourceCollection.xml,
省却手动配置的麻烦。

#### 使用说明

1. 假设你本地已有 [GameFramework](https://github.com/EllanJiang/GameFramework.git) 或者 [StarForce](https://github.com/EllanJiang/StarForce.git) 项目。

2. Clone本仓库,拷贝Assets/GameMain/Scripts/Editor/ResourceRuleEditor 文件夹到项目Editor文件夹中,等待编译完成。

3.  打开菜单 GameFramework/Resource Tools/Resource Rule Editor,出现如图所示的窗口。
	![avatar](/imgs/Editor.png)
	
4. 点击 Add 或 + ,添加一条规则记录, 指定一个文件夹,并分配 过滤类型(FilterType) 和 搜索模式(Patterns) ,支持可选参数: 资源组列表(Groups),
   AB变体(Varient),打进包内(Packed)。

5. 使用Editor Dirty技术,会自动保存编辑器数据。也可以点击 Save 按钮手动保存。编辑完成后,点击 Refresh ResourceCollection.xml 按钮刷新。这个过程可以自动化,
   在合适的地方请求一次刷新: ```ResourceRuleEditor.GetWindow<ResourceRuleEditor>().RefreshResourceCollection();```

   新增	`ResourceRuleEditorUtility.RefreshResourceCollection` 方法刷新ResourceCollection.xml  不会弹出RuleEditorWindow

6.  过滤类型(FilterType)说明:  
		Root 是指定文件夹打成一个Resource。  
		Children 指定文件夹下的文件分别打成一个Resource。  
		Children Folders Only 指定文件夹下的子文件夹分别打成一个Resource。  
		Children Files Only 指定文件夹下的子文件夹的文件分别打成一个Resource。  
	
7. Groups,Pattens 多个参数值可以用 "," , ";" , "|" 来分割。

8. 名称(Name)相同时,则表示资源打入同一个Resource。

9.  名称(Name)可缺省。  
		当过滤类型选择为 Root 或 Children Folders Only, Resource名称会自动命名为文件夹的名称。  
		当过滤类型选择为 Children 或 Children Files Only,Resource名称会自动命名为文件的名称。  

#### 多个Rule配置使用说明

![multconfig](/imgs/MultConfigEditor.png)

1. 基础使用说明和上面一致
2. 切换配置的方式 
   + 单击CurrentConfig 选择框  选择不同的配置。 
   + Project界面选中想要切换的ScriptableObject配置。 在ScriptableObejct 的检视面板 Open按钮也可以直接打开RuleEditor
3. 手动复制新增配置时  界面需要刷新才能识别到 点击relaoad 按钮。 或者直接选中新增的配置 会自动刷新。 
4. 代码刷新ResourceCollection.xml  调用`ResourceRuleEditorUtility.RefreshResourceCollection(string configPath)`

