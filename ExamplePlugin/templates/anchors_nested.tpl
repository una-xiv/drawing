<style>
#main {
    anchor: top-left;
    size: 500 500;
    background-color: 0xFF212121;
    border-radius: 10;
    padding: 10;
    gap: 10;
    
    .item {
        size: 128;
        background-color: 0xFF424242;
        background-gradient: radial 0x40FFFFFF 0x00000000;
        border-radius: 10;
        shadow-size: 32;
        shadow-inset: 4;
        
        text-align: middle-center;
        color: 0xFF000000;
        font: 1;
        font-size: 14;
        outline-color: 0xA0FFFFFF;
        outline-size: 1;
        is-antialiased: false;
        text-offset: 0 -25;
        padding: 5;
        
        .child {
            padding: 5;
            text-align: middle-center;
            color: 0xFFFFFFFF;
            background-color: 0xFF212021;
            border-radius: 8;
            shadow-size: 12;
            shadow-inset: 4;
        }
    }
}

.top-left { anchor: top-left; }
.top-center { anchor: top-center; }
.top-right { anchor: top-right; }
.middle-left { anchor: middle-left; }
.middle-center { anchor: middle-center; }
.middle-right { anchor: middle-right; }
.bottom-left { anchor: bottom-left; }
.bottom-center { anchor: bottom-center; }
.bottom-right { anchor: bottom-right; }
</style>

<node id="main">
    <node class="item top-left" value="Top Left">
        <node class="child top-left" value="TL"/>
        <node class="child top-center" value="TC"/>
        <node class="child top-right" value="TR"/>
        <node class="child middle-left" value="ML"/>
        <node class="child middle-center" value="MC"/>
        <node class="child middle-right" value="MR"/>
        <node class="child bottom-left" value="BL"/>
        <node class="child bottom-center" value="BC"/>
        <node class="child bottom-right" value="BR"/>
    </node>
    
    <node class="item top-center" value="Top Center">
        <node class="child top-left" value="TL"/>
        <node class="child top-center" value="TC"/>
        <node class="child top-right" value="TR"/>
        <node class="child middle-left" value="ML"/>
        <node class="child middle-center" value="MC"/>
        <node class="child middle-right" value="MR"/>
        <node class="child bottom-left" value="BL"/>
        <node class="child bottom-center" value="BC"/>
        <node class="child bottom-right" value="BR"/>
    </node>
    
    <node class="item top-right" value="Top Right">
        <node class="child top-left" value="TL"/>
        <node class="child top-center" value="TC"/>
        <node class="child top-right" value="TR"/>
        <node class="child middle-left" value="ML"/>
        <node class="child middle-center" value="MC"/>
        <node class="child middle-right" value="MR"/>
        <node class="child bottom-left" value="BL"/>
        <node class="child bottom-center" value="BC"/>
        <node class="child bottom-right" value="BR"/>
    </node>
    
    <node class="item middle-left" value="Middle Left">
        <node class="child top-left" value="TL"/>
        <node class="child top-center" value="TC"/>
        <node class="child top-right" value="TR"/>
        <node class="child middle-left" value="ML"/>
        <node class="child middle-center" value="MC"/>
        <node class="child middle-right" value="MR"/>
        <node class="child bottom-left" value="BL"/>
        <node class="child bottom-center" value="BC"/>
        <node class="child bottom-right" value="BR"/>
    </node>
    
    <node class="item middle-center" value="Middle Center">
        <node class="child top-left" value="TL"/>
        <node class="child top-center" value="TC"/>
        <node class="child top-right" value="TR"/>
        <node class="child middle-left" value="ML"/>
        <node class="child middle-center" value="MC"/>
        <node class="child middle-right" value="MR"/>
        <node class="child bottom-left" value="BL"/>
        <node class="child bottom-center" value="BC"/>
        <node class="child bottom-right" value="BR"/>
    </node>
    
    <node class="item middle-right" value="Middle Right">
        <node class="child top-left" value="TL"/>
        <node class="child top-center" value="TC"/>
        <node class="child top-right" value="TR"/>
        <node class="child middle-left" value="ML"/>
        <node class="child middle-center" value="MC"/>
        <node class="child middle-right" value="MR"/>
        <node class="child bottom-left" value="BL"/>
        <node class="child bottom-center" value="BC"/>
        <node class="child bottom-right" value="BR"/>
    </node>
    
    <node class="item bottom-left" value="Bottom Left">
        <node class="child top-left" value="TL"/>
        <node class="child top-center" value="TC"/>
        <node class="child top-right" value="TR"/>
        <node class="child middle-left" value="ML"/>
        <node class="child middle-center" value="MC"/>
        <node class="child middle-right" value="MR"/>
        <node class="child bottom-left" value="BL"/>
        <node class="child bottom-center" value="BC"/>
        <node class="child bottom-right" value="BR"/>
    </node>
    
    <node class="item bottom-center" value="Bottom Center">
        <node class="child top-left" value="TL"/>
        <node class="child top-center" value="TC"/>
        <node class="child top-right" value="TR"/>
        <node class="child middle-left" value="ML"/>
        <node class="child middle-center" value="MC"/>
        <node class="child middle-right" value="MR"/>
        <node class="child bottom-left" value="BL"/>
        <node class="child bottom-center" value="BC"/>
        <node class="child bottom-right" value="BR"/>
    </node>
    
    <node class="item bottom-right" value="Bottom Right">
        <node class="child top-left" value="TL"/>
        <node class="child top-center" value="TC"/>
        <node class="child top-right" value="TR"/>
        <node class="child middle-left" value="ML"/>
        <node class="child middle-center" value="MC"/>
        <node class="child middle-right" value="MR"/>
        <node class="child bottom-left" value="BL"/>
        <node class="child bottom-center" value="BC"/>
        <node class="child bottom-right" value="BR"/>
    </node>
</node>
