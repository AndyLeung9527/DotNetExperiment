### 创建模板包

将一个或多个模板（Item或Project模板）打包到NuGet（.nupkg）包，安装或卸载模板包时，将添加或删除模板包中包含的所有模板

#### 一、NuGet包源

NuGet包源需要安装Microsoft.TemplateEngine.Authoring.Templates

```cmd
dotnet new install Microsoft.TemplateEngine.Authoring.Templates
```

#### 二、创建模板包项目

在工作目录下创建模板包

```cmd
dotnet new templatepack -n "AdatumCorporation.Utility.Templates"
```

得到下列信息则创建成功

```cmd
已成功创建模板“模板包”。

正在处理创建后操作...
说明: 需要手动操作
手动说明: 在编辑器中打开 *.csproj 并完成包元数据配置。将模板复制到 "content" 文件夹。填写 README.md。
```

修改AdatumCorporation.Utility.Templates.csproj文件，填充包信息和模板属性等

#### 三、准备好模板

示例中已有DotNetTemplate.Item和DotNetTemplate.Project模板，将其复制到模板包项目的`content`文件夹去（其中模板自带的`SampleTemplate`文件夹可以删除）

```
DotNetTemplate
│   AdatumCorporation.Utility.Templates.csproj
└───content
    ├───DotNetTemplate.Item
    │   └───.template.config
    │           template.json
    └───DotNetTemplate.Project
        └───.template.config
                template.json
```

#### 四、打包

在`AdatumCorporation.Utility.Templates.csproj`的目录下执行

```cmd
dotnet pack
```

之后会在`\bin\Release`下生成NuGet包`AdatumCorporation.Utility.Templates.1.0.0.nupkg`

#### 五、安装

```cmd
dotnet new install .\bin\Release\AdatumCorporation.Utility.Templates.1.0.0.nupkg
```

卸载：

```cmd
dotnet new uninstall AdatumCorporation.Utility.Templates
```

