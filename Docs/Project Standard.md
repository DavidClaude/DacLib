# Project Standard

## 一、文件命名规范

- 使用首字母大写的驼峰式命名规则

- 对于类，文件名与文件内类名(主类名)一致

- 对于静态方法集，文件名使用"XxxFunc"形式

- 对于委托、枚举等集合，即一个文件包含多个类或其它对象，文件名使用复数形式，如GenericDelegates

- 测试脚本使用"TS_Xxx"形式，可删除

- 作为测试用例的非代码文件使用"SAMPLE_Xxx"形式，可删除

- Archits中的文件，采用统一前缀方式命名，如GameObjectPool系统中的GOPool(主体)和GOPoolElem

- 核心框架中的文件，采用统一前缀加下划线方式命名，如Hoxis框架中的Hoxis_Client、Hoxis_Agent

- 对于框架文件夹下的对象集合，数量不多的情况下可使用"XxxUtilities"命名

## 二、文件层级规范

- **Generic**文件夹中存放最基础的通用文件，如公共方法集、委托类型、枚举、常量等

- **Docs**文件夹中存放可读文档

- **Codex**文件夹中存放具有特定功能的类，一个文件对应一个完整功能

- **Archits**文件夹中存放具有独立运行能力的系统(包含Mono)，每个系统单独创建文件夹，如"FiniteStateMachine"

- **Lib**文件夹中存放动态库，如.dll文件

- 框架文件夹可直接存放于DacLib下，鉴于其文件会多次多处使用，具有独特命名空间，如Hoxis

- 文件夹使用首字母大写的驼峰式命名规则

- 命名空间在规范作用范围内严格按照目录结构划分，其余可根据需求自由设置

## 三、编码命名规范

- 属性使用get;set访问器封装，使用首字母小写的驼峰式命名规则，如：

```c#
public string charaName {get;}
```

字段以"_"为前缀，首字母小写，遵循驼峰命名，可做初始化，如：

```c#
private int _curIndex = 0;
```

函数首字母大写，遵循驼峰命名，如：

```c#
public string GetIndex () {}
```

- 委托使用GenericDelegate提供的格式化委托类型，个别特殊定制的签名，由使用方自行定义

- 事件使用"onXxx"形式，并使用"OnXxx"方法封装，如：

```c#
public event NoneForVoid_Handler onHPDepleted;
public void OnHPDepleted ()
{
    if (onHPDepleted == null)
    	return;
    onHPDepleted();
}
```

- 方法中的形式参数尽量使用具有意义的单词，如"data"

- 构造方法中参数遵循方法形式参数规则，若与属性冲突，使用变量名+Arg的形式，如：

```c#
public string name {get;}
public Item (string nameArg)
{
    name = nameArg;
}
```

- 临时变量使用单词简写，如"dis"

- 迭代器中的临时变量使用单个字母，如：

```c#
foreach (string s in layers) {}
```

## 四、编码设计规范

### 自建库文件层级

- Generic为最底层库，只允许封装.Net和非开源动态库(.dll)；只允许Func文件引用Generic基础对象

- Codex只允许使用Generic服务
- 框架可使用所有服务
- U3D可使用所有服务，包括框架，其内部文件结构参考上述原则

### 关键字使用策略

#### [类与结构体]

- 具有属性、方法的能够独立完成单元功能的对象使用class定义，引用类型能够避免不必要的复制

- 仅将若干类型集合起来便于使用的对象使用struct定义，可理解为基本类型的扩展，如：

```c#
public struct KV<TK,TV>
{
    public TK key;
    public TV val;
}
```

#### [访问权限]

- class属性使用{get;set;}封装，一般使用简写，若需要进行处理再展开方法域

- struct字段不使用封装，便于序列化与反序列化，对于只读字段使用readonly修饰

- class的只读属性在构造方法中赋值，struct若要对只读字段赋值，必须提供构造方法，否则可以使用初始化赋值

```c#
public KV<string,string> field = new KV<string,string>
{
    key = "key",
    val = "value"
}
```

#### [常量、静态量]

- 基本类型常量使用const修饰，引用型静态只读变量使用static readonly修饰，进行初始化赋值

- struct须定义"undef"表示无效值(因为struct类型不能为null)，使用"ok"表示正常值，如：

```c#
//无效的HoxisID
public static readonly HoxisID undef = new HoxisID("", 0);
//正确的返回结果
public static readonly Ret ok = new Ret(LogLevel.Info, 0, "");
```

### 错误处理

- 对于期望返回结构化结果的方法，添加out Ret ret参数

- 须指定报错级别

   --INFO：信息，符合使用要求且不影响运行，使用者希望获取自定义信息

   --WARNING：警告，不符合使用要求、运行时出现告警，但不影响运行

   --ERROR：错误，无法完成预期功能，甚至导致运行崩溃

- 错误码以常量形式定义于相关类中，统一使用"RET"前缀

- 描述使用一般英文陈述，字符串格式应为"File:sample_toml.toml doesn't exist"

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
            LogLevel.Error,
            ERROR_NO_FILE,
            "File:" + path + " doesn't exist"
        );
        //if sucess
        ret = Ret.ok;
    }
}
```

- 对于编译器可能报错的代码段，使用try&catch包装，并结合Ret使用，Ret描述中将错误信息加上，遵循“调用方不需try&catch，仅关心Ret”的原则

```c#
public void Try(string str, out Ret ret)
{
    try { /.../ }
    catch (Exception e)
    {
        ret = new Ret(LogLevel.Error, 1,"...\n" + e.Message);	//捕获错误时，Ret报错并返回
        return;
    }
    ret = Ret.ok;	//正常结束时，Ret为ok
}
```

### 注释规范

- 使用英文描述属性、方法等公共成员，对于重要私有成员也可进行注释

- 运行于线程中的方法，使用*\*WITHIN THREAD**作为注释首行，用于提示使用者对临界资源做lock操作

- 一般对大段常量定义、私有方法使用region折叠，region名为小写复数形式，如"private functions"

### 成员顺序

- 变量：

常量 -> 只读字段 -> 静态量 -> 公共属性(简写) -> 公共属性(完整) -> 公共事件 -> 私有字段 -> 私有事件

- 方法：

构造方法 -> 公共方法 -> 私有方法

一般方法 -> 事件调用方法 -> 回调方法 -> 完全初始化方法(Init)

方法 -> 重载方法

```c#
public class SampleClass
{
    #region readonly
    public const string SAMPLE_DESC = "sample class"; 
    public readonly float rate = 1.0f;
    #endregion
        
    #region properties
    public static int index {get;set;}
    public string name {get;private set;}
    public int count { get{ return _list.Count; } }
    public event SampleHandler onEvent;
    #endregion
    
    #region fields
    private List<string> _list;
    private event SampleHandler _onPrivateEvent;
    #endregion
    
    public SampleClass(string nameArg) {
        name = nameArg;
    }
    
    public void Update(float f) {
        //...
    }
    public void Update() { Update(1.0f); }
    
    public void OnEvent() {
        if (onEvent == null)
            return;
        onEvent();
    }
    
    public void CallCb() {}
    
    public void Init() {}
    
    private void GetIndex() {}
    private void OnPrivateEvent() {
        if (_onPrivateEvent == null)
            return;
        _onPrivateEvent();
    }
}
```

## 五、软件系统设计

### 节点模型单元

#### [监控]

- 说明：Observe。A对B具有有限管理权；B为具有处理、输出数据能力的独立对象；支持动态关联、分离
- 层级：A高于B
- 规则：A可调用B的方法，不允许逆向调用，A可使用观察者模式对B进行监控
- 实例：Hoxis中Connection包含User

#### [包含]

- 说明：Include。B是A的成员，A对B可全权管理；B一般为静态的数据集加基本方法
- 层级：B属于A
- 规则：A可调用B的方法，不允许逆向调用，B不处理数据或仅在内部处理
- 实例：任何引用了Codex的对象

#### [调用]

- 说明：Call。B提供了某功能模块的实现方法，A仅负责调用，不关心执行结果
- 层级：A高于B
- 规则：A对B单向调用，不监控不逆向
- 实例：Hoxis中的Controller调用Agent，Agent调用Behaviour

#### [双工]

- 说明：Duplex。A为双工节点，一般为进行双向通信的调度器，可接受调用，处理完成后返回相关信息，或者再调用其它对象
- 层级：A为单例，单例间为平级关系
- 规则：被调用时需保证“处理->输出”的过程，避免穿透，即B借助A的调用权限间接调用C
- 实例：Hoxis中的Client与Director

### 标准化设计：属性

### 标准化设计：方法

#### [Initialize]

- 说明：实例被创建时会调用构造方法来初始化属性、字段，Initialize则是将实例还原至初始化状态
- 场景：期望在不频繁创建、释放的情况下复用对象
- 实例：某成员数组在构造函数中被创建，可在Initialize中使用初始化方法进行初始化

#### [Awake - Pause - Continue - Reset]

- 说明：Awake用于开启功能；Pause用于暂停运转，但须保持状态不变；Continue用于继续运转；Reset用于关闭功能，状态须重置
- 场景：对象中包含多个可运转功能，需要依据主体状态控制其工作
- 实例：Hoxis中User在被启用时需开启心跳检测，在停用时关闭

#### [Start - Stop]

- 说明：单一功能的开启与关闭
- 场景：对象本身即为单一可运转功能的实现
- 实例：时段计时器

#### [Begin - End]

- 说明：BeginXxx用于开启线程或开辟缓冲的操作；EndXxx用于结束该操作，并释放资源
- 场景：期望实现需申请额外资源的操作，在Begin中初始化，在End中中止，保证资源回收
- 实例：异步计时器、调试日志

#### [SignIn - SignOut]

- 说明：数据填充

#### [Connect - Close]

