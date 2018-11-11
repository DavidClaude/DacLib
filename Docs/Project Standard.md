# Project Standard

## 一、文件命名规范

● 使用首字母大写的驼峰式命名规则

● 对于类，文件名与文件内类名(主类名)一致

● 对于静态方法集，文件名使用"XxxFunc"形式

● 对于委托、枚举等集合，文件名使用复数形式，如GenericDelegates

● 测试脚本使用"TS_Xxx"形式，可删除

● 作为测试用例的非代码文件使用"SAMPLE_Xxx"形式，可删除

● Archits中的文件，采用统一前缀方式命名，如GameObjectPool系统中的GOPool(主体)和GOPoolElem

● 核心框架中的文件，采用统一前缀加下划线方式命名，如Hoxis框架中的Hoxis_Client、Hoxis_Agent

## 二、文件层级规范

● **Generic文件夹**中存放最基础的通用文件，如公共方法集、委托类型、枚举、常量等

● **Docs文件夹**中存放可读文档

● **Codex文件夹**中存放具有特定功能的类，一个文件对应一个完整功能

● **Archits文件夹**中存放具有独立运行能力的系统(包含Mono)，每个系统单独创建文件夹，如"FiniteStateMachine"

● **Lib文件夹**中存放动态库，如.dll文件

● 核心框架可直接存放于DacLib下，其文件会被使用多次且可能用于任何地方，具有独特命名空间，如Hoxis

● 文件夹使用首字母大写的驼峰式命名规则

● 命名空间在规范作用范围内严格按照目录结构划分，其余可根据需求自由设置

## 三、编码命名规范

● 属性使用get;set访问器封装，使用首字母小写的驼峰式命名规则，如：

```c#
public string itemName {get;}
```

字段以"_"为前缀，首字母小写，遵循驼峰命名，可做初始化，如：

```c#
private int _currentIndex = 0;
```

函数首字母大写，遵循驼峰命名，如：

```c#
public string GetDesc () {}
```

● 委托使用GenericDelegates提供的格式化委托类型，个别特殊定制的签名，由使用方自行定义

● 事件使用"onXxx"形式，并使用"OnXxx"方法封装，如：

```c#
public event NoneForVoid_Handler onHPDepleted;
public void OnHPDepleted ()
{
    if (onHPDepleted == null)
    	return;
    onHPDepleted();
}
```

● 构造方法中参数使用变量名+Arg的形式，如：

```c#
public string name {get;}
public Item (string nameArg)
{
    name = nameArg;
}
```

● 方法中的参数尽量使用具有意义的单词，如"data"

● 临时变量使用单词简写，如"dis"

● 循环中的临时变量使用单个字母，如：

```c#
foreach (string s in layers) {}
```

## 四、编码设计规范

### 错误处理

● 对于期望返回结构化结果的方法，添加out Ret ret参数

● 须指定报错级别

​	--INFO：信息，符合使用要求且不影响运行，使用者希望获取自定义信息

​	--WARNING：警告，不符合使用要求但不影响运行

​	--ERROR：错误，可能导致运行崩溃

● 错误码以常量形式定义于相关类中，统一使用"RET"前缀

```c#
public class RetClass
{
    public const int RET_NO_FILE = 1;
    public const int RET_FILE_NOT_USED = 2;
    public const int RET_FILE_NAME = 3;
    
    public void ReadFile (string path, out Ret ret)
    {
        //if error: no file
        ret = new Ret (
            RetLevel.Error,
            ERROR_NO_FILE,
            "File:" + path + " doesn't exist"
        );
        //if sucess
        ret = Ret.ok;
    }
}
```

