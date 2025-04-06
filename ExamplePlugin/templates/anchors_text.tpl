<style>
#main {
    flow: vertical;
    gap: 10;
    
    .row {
        flow: horizontal;
        gap: 10;
        auto-size: grow;
        
        .text {
            auto-size: grow;
            background-color: 0xFF212021;
            border-radius: 8;
            padding: 10;
        }
    }
}

.top-left { text-align: top-left; }
.top-center { text-align: top-center; }
.top-right { text-align: top-right; }
.middle-left { text-align: middle-left; }
.middle-center { text-align: middle-center; }
.middle-right { text-align: middle-right; }
.bottom-left { text-align: bottom-left; }
.bottom-center { text-align: bottom-center; }
.bottom-right { text-align: bottom-right; }
</style>

<node id="main">
    <node class="row">
        <node class="text top-left" value="Top Left" />
        <node class="text top-center" value="Top Center" />
        <node class="text top-right" value="Top Right" />
    </node>
    <node class="row">
        <node class="text middle-left" value="Middle Left" />
        <node class="text middle-center" value="Middle Center" />
        <node class="text middle-right" value="Middle Right" />
    </node>
    <node class="row">
        <node class="text bottom-left" value="Bottom Left" />
        <node class="text bottom-center" value="Bottom Center" />
        <node class="text bottom-right" value="Bottom Right" />
    </node>
</node>