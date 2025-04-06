<style>
#main {
    flow: vertical;
    size: 256 128;
    background-color: 0x80000000;
    padding: 5 16 5 5;
    gap: 5;
    
    .text {
        auto-size: grow fit;
        word-wrap: true;
        color: 0xFFFFFFFF;
        outline-color: 0xFF000000;
        outline-size: 1;
        padding: 5;
        stroke-color: 0xFF0000AA;
        stroke-width: 1;
    }
}
</style>

<node id="main" overflow=false>
    <node class="text" value="Multiple elements in a scrollable container."/>
    <node class="text" value="Does it really work this time?"/>
    <node class="text" value="Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi porttitor, nisl finibus suscipit ultricies, nibh mi luctus felis, in tincidunt nibh ligula at lorem. Vivamus consequat lacinia mauris, eu vulputate neque dignissim ac. Sed ut rutrum lacus. Fusce nec molestie purus. Sed luctus lacus eget ex tempor lacinia. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Ut at tincidunt risus. Aliquam cursus augue sem, eu vehicula sapien egestas tristique. Proin rutrum eget nisi lobortis scelerisque."/>
</node>