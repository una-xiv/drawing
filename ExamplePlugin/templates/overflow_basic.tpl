<style>
#main {
    flow: vertical;
    anchor: top-left;
    size: 640 480;
    
    .header {
        size: 0 48;
        auto-size: grow fit;
        anchor: top-left;
        background-color: 0xff383838;
        border-color: 0xff787a7a;
        border-width: 0 0 1 0;
        padding: 0 10;
        gap: 10;
        
        .icon {
            anchor: middle-left;
            size: 32 32;
        }
        
        .title {
            anchor: middle-left;
            size: 0 32;
            auto-size: grow fit;
            color: 0xffffffff;
            text-align: middle-left;
            font-size: 16;
        }
    }
    
    .body {
        auto-size: grow grow;
        anchor: top-left;
        background-color: 0xff212021;
        
        .scroller {
            size: 0 0;
            auto-size: grow grow;
            padding: 5;
            
            .container {
                flow: vertical;
                anchor: top-left;
                auto-size: grow fit;
                background-color: 0x40000000;
                padding: 10;
                gap: 5;
                
                .text {
                    auto-size: grow fit;
                    color: 0xffffffff;
                    line-height: 1.25;
                    word-wrap: true;
                    text-overflow: true;
                }
                
                .list-item {
                    text-align: middle-left;
                    color: 0xffffffff;
                }
            }
        }
    }
    
    .footer {
        anchor: top-left;
        size: 0 48;
        auto-size: grow fit;
        background-color: 0xff383838;
        border-color: 0xff787a7a;
        border-width: 1 0 0 0;
        padding: 0 10;
    }
}
</style>

<node id="main">
    <node class="header">
        <node class="icon" style={icon-id: 111;}/>
        <node class="title" value="Overflow Test"/>
    </node>
    <node class="body">
        /**
         * A scrolling container requires an extra node that contains the ImGui child frame.
         * This container should then contain exactly ONE element that represents the content
         * of the scrolling container.
         *
         * This setup is required for performance reasons, since we need to reposition the
         * content of the scrolling container whenever the user scrolls through the content.
         */
        <node class="scroller" overflow=false>
            <node class="container">
                <node class="text" value="This is a test of the overflow container. It should be able to scroll through the content if it is too large to fit in the available space. The text should wrap correctly and not overflow the container."/>
                <node class="list-item" value="This is the first item inside the overflow container."/>
                <node class="list-item" value="This is the second item inside the overflow container."/>
                <node class="list-item" value="This is the third item inside the overflow container."/>
                <node class="list-item" value="This is the fourth item inside the overflow container."/>
                <node class="list-item" value="This is the fifth item inside the overflow container."/>
                <node class="list-item" value="This is the sixth item inside the overflow container."/>
                <node class="list-item" value="This is the seventh item inside the overflow container."/>
                <node class="list-item" value="This is the eighth item inside the overflow container."/>
                <node class="list-item" value="This is the ninth item inside the overflow container."/>
                <node class="text" value="This is another test of the overflow container. It should be able to scroll through the content if it is too large to fit in the available space. The text should wrap correctly and not overflow the container."/>
                <node class="text" value="Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi porttitor, nisl finibus suscipit ultricies, nibh mi luctus felis, in tincidunt nibh ligula at lorem. Vivamus consequat lacinia mauris, eu vulputate neque dignissim ac. Sed ut rutrum lacus. Fusce nec molestie purus. Sed luctus lacus eget ex tempor lacinia. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Ut at tincidunt risus. Aliquam cursus augue sem, eu vehicula sapien egestas tristique. Proin rutrum eget nisi lobortis scelerisque."/>
                <node class="list-item" value="This is the tenth item inside the overflow container."/>
                <node class="list-item" value="This is the eleventh item inside the overflow container."/>
                <node class="list-item" value="This is the twelfth item inside the overflow container."/>
                <node class="list-item" value="This is the thirteenth item inside the overflow container."/>
                <node class="list-item" value="This is the fourteenth item inside the overflow container."/>
                <node class="list-item" value="This is the fifteenth item inside the overflow container."/>
                <node class="list-item" value="This is the sixteenth item inside the overflow container."/>
            </node>
        </node>
    </node>
    <node class="footer">
        <node value="This is the footer"/>
    </node>
</node>