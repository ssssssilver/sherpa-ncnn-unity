# Unity发布的WebGL支持录音  

csdn/bilibili @阴沉的怪咖  
哔哩哔哩主页：https://space.bilibili.com/45077877?spm_id_from=333.1007.0.0

# 解决方案说明
因为这个项目涉及到麦克风录制声音，用作语音识别。在所有的语音识别接口都改成web api之后，发现unity发布到webgl失败了，查了一下相关资料，原来是unity内置的microphone类是不支持webgl了，所以只好另找解决方案。在网上找了几个解决方案，实测了一个博主的源码，的确解决了我的问题，所以就把他的解决方案集成到我的项目里了。

方案的思路，参见unity官方文档，文档里描述了unity如何调用js的方法，涉及到unity端*.jslib的拓展方法
unity文档：https://docs.unity3d.com/cn/2020.3/Manual/webgl-interactingwithbrowserscripting.html

我参考的博主的解决方案里，除了在unity端调用js代码外，还有js回传数据到unity。部分代码实现是在js里实现了，所以在发布webgl后，需要修改一点代码，并加入js库，具体配置方法，见下文。

# 参考资料：

CSDN博客：https://blog.csdn.net/Wenhao_China/article/details/126779212?spm=1001.2014.3001.5502t
CSDN博文：https://blog.csdn.net/a987654sd/article/details/105551560

解决方案作者的源码地址：
Github：https://github.com/HiWenHao/UnityWebGLMicrophone


附加材料：
以下两个webgl使用microphone方案我尚未验证，也放在这里供参考

解决方案1：https://gitcode.net/mirrors/xiangyuecn/recorder?utm_source=csdn_github_accelerator
解决方案2：https://github.com/tgraupmann/UnityWebGLMicrophone/tree/master

## 版本
**Unity 2020.3.44
Microsoft Visual Studio Professional 2022**

## 发布到webgl说明

注意，发布webgl时，工程文件的路径必须是全英文。
- playersetting设置
1、Other Settings里,Color Space修改为Gamma
2、Publishing Settings里，勾选Decompression Fallback

## js脚本配置

- 添加js脚本
1、在Plugins目录下找到JavaScripts文件夹，把文件夹下的[recorder.wav.min.js]拷贝到输出的webgl包，index.html相同的文件夹下[根目录]

-修改index.html文件

1、在Plugins目录下找到JavaScripts文件夹，找到[AddToIndex.js]文件，后续需要添加的代码都在这个文件里了，直接复制就可以了
2、用代码编辑器打开index.html文件
3、[AddToIndex.js]里拷贝"<script src="./recorder.wav.min.js"></script>",添加到index.html里，引用[recorder.wav.min.js]脚本
4、[AddToIndex.js]里拷贝第7行到110行的代码，到<script>脚本里（可以直接添加到" document.body.appendChild(script);"这行代码后面）
5、[AddToIndex.js]里拷贝"UnityIns = unityInstance;  initRecord();" 这两行代码，复制到unityInstance实例化的代码里（可以添加到"then((unityInstance) => {" 这段代码之后）

上述配置完成，就可以部署实测了

## webgl站点部署

推荐PhpStudy，一键部署