<style>
#main {
    anchor: top-left;
    size: 900 300;
    flow: vertical;
    background-color: 0xff212021;
    stroke-color: 0xff787a7a;
    stroke-width: 1;
    border-radius: 8;
    padding: 1;
    drop-shadow: 0 0 16;
    
    .header {
        size: 0 48;
        auto-size: grow fit;
        anchor: top-left;
        background-color: 0xff383838;
        border-color: 0xff787a7a;
        border-width: 0 0 1 0;
        gap: 10;
        rounded-corners: top-left top-right;
        border-radius: 8;
        padding: 1;

        .gradient {
            anchor: top-center;
            size: 0 24;
            auto-size: grow fit;
            background-gradient: vertical 0xff686838 0x00000000;
            rounded-corners: top-left top-right;
            border-radius: 8;
        }

        .left-side {
            anchor: middle-left;
            flow: horizontal;
            auto-size: grow fit;
            padding: 0 10;
            gap: 10;
            
            .icon {
                anchor: middle-left;
                size: 32 32;
                drop-shadow: 0 0 8;
            }
    
            .title {
                anchor: middle-left;
                size: 0 32;
                auto-size: grow fit;
                color: 0xffffffff;
                text-align: middle-left;
                font-size: 16;
                outline-color: 0xff000000;
                outline-size: 1;
            }
        }
        
        .right-side {
            anchor: middle-left;
            flow: horizontal;
            gap: 4;
            padding: 0 10;
            
            .button {
                anchor: middle-right;
                size: 25;
                background-color: 0xff191919;
                stroke-color: 0xff000000;
                stroke-width: 1;
                border-color: 0xff787a7a;
                border-width: 1;
                border-inset: 2;
                border-radius: 5;
                text-align: middle-center;
                color: 0xffa0a0a0;
                
                &:hover {
                    background-color: 0xff383838;
                    stroke-color: 0xffa0a0a0;
                    color: 0xffffffff;
                    drop-shadow: 0 0 8;
                }
            }
        }
    }
    
    .body {
        auto-size: grow grow;
        background-color: 0xff191919;
        
        .aside {
            size: 300 0;
            auto-size: fit grow;
            background-color: 0xff212021;
            border-color: 0xff787a7a;
            border-width: 0 1 0 0;
            
            .scroller {
                auto-size: grow grow;
            
                .content {
                    flow: vertical;
                    padding: 10;
                    auto-size: grow fit;
                    gap: 5;
                    
                    .logo {
                        size: 256 48;
                        icon-id: 120951;
                    }
                    
                    .menu-item {
                        size: 0 32;
                        auto-size: grow fit;
                        gap: 8;
                        padding: 0 10;
                        border-radius: 8;
                        
                        .icon {
                            anchor: middle-left;
                            size: 16;
                            icon-id: 111;
                        }
                        .text {
                            anchor: middle-left;
                            auto-size: grow fit;
                            size: 0 32;
                            color: 0xffffffff;
                            text-align: middle-left;
                            font-size: 13;
                        }
                        .alt {
                            anchor: middle-left;
                            text-align: middle-right;
                            font-size: 12;
                            color: 0xff787a7a;
                        }
                        
                        &:hover {
                            background-color: 0xffcacaca;
                            drop-shadow: 1 1 7;
                            
                            .text {
                                color: 0xff000000;
                            }
                            
                            .alt {
                                color: 0xff000000;
                            }
                        }
                    }
                }
            }
        }
        
        .content {
            flow: vertical;
            padding: 10;
            gap: 10;
            auto-size: grow grow;
            
            .inner {
                auto-size: grow grow;
                padding: 10;
                
                &.top {
                    background-color: 0x30ff0000;
                    text-align: top-center;
                    font-size: 16;
                    text-overflow: false;
                }
                &.bottom {
                    background-color: 0x3000ff00;
                    text-align: bottom-center;
                    font-size: 16;
                    text-overflow: false;
                }
            }
        }
    }
    
    .footer {
        auto-size: grow fit;
        background-color: 0xff383838;
        border-color: 0xff787a7a;
        border-width: 1 0 0 0;
        padding: 10;
        rounded-corners: bottom-left bottom-right;
        border-radius: 8;
    }
}
</style>

<node id="main">
    <node class="header">
        <node class="gradient" />
        <node class="left-side">
            <node class="icon" style={icon-id: 113;} />
            <node class="title" value="Auto-sized layouts are cool" />
        </node>
        <node class="right-side">
            <node class="button" value="X" />
            <node class="button" value="Y" />
            <node class="button" value="Z" />
        </node>
    </node>
    <node class="body">
        <node class="aside">
            <node class="scroller" overflow=false>
                <node class="content">
                    <node class="logo"/>
                    
                    <node class="menu-item">
                        <node class="icon" style={icon-id: 111;} />
                        <node class="text" value="Menu Item 1" />
                        <node class="alt" value="Alt text" />
                    </node>
                    <node class="menu-item">
                        <node class="icon" style={icon-id: 111;} />
                        <node class="text" value="Menu Item 2" />
                        <node class="alt" value="Alt text" />
                    </node>
                    <node class="menu-item">
                        <node class="icon" style={icon-id: 111;} />
                        <node class="text" value="Menu Item 3" />
                        <node class="alt" value="Alt text" />
                    </node>
                    <node class="menu-item">
                        <node class="icon" style={icon-id: 111;} />
                        <node class="text" value="Menu Item 4" />
                        <node class="alt" value="Alt text" />
                    </node>
                    <node class="menu-item">
                        <node class="icon" style={icon-id: 111;} />
                        <node class="text" value="Menu Item 5" />
                        <node class="alt" value="Alt text" />
                    </node>
                    <node class="menu-item">
                        <node class="icon" style={icon-id: 111;} />
                        <node class="text" value="Menu Item 6" />
                        <node class="alt" value="Alt text" />
                    </node>
                    <node class="menu-item">
                        <node class="icon" style={icon-id: 111;} />
                        <node class="text" value="Menu Item 7" />
                        <node class="alt" value="Alt text" />
                    </node>
                    <node class="menu-item">
                        <node class="icon" style={icon-id: 111;} />
                        <node class="text" value="Menu Item 8" />
                        <node class="alt" value="Alt text" />
                    </node>
                    <node class="menu-item">
                        <node class="icon" style={icon-id: 111;} />
                        <node class="text" value="Menu Item 9" />
                        <node class="alt" value="Alt text" />
                    </node>
                    <node class="menu-item">
                        <node class="icon" style={icon-id: 111;} />
                        <node class="text" value="Menu Item 10" />
                        <node class="alt" value="Alt text" />
                    </node>
                </node>
            </node>
        </node>
        <node class="content">
            <node class="inner top" value="Top of the morning to you!">
            </node>
            <node class="inner bottom" value="Bottom of the evening to you!">
            </node>
        </node>
    </node>
    <node class="footer">
        <node value="This is the footer"/>
    </node>
</node>