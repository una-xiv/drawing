<style>
.grow-h {
    auto-size: grow fit;
}
.grow-v {
    auto-size: fit grow;
}
.grow-both {
    auto-size: grow grow;
}
.horizontal {
    flow: horizontal;
}
.vertical {
    flow: vertical;
}
.c {
    padding: 10;
    gap: 10;
    color: 0xffffffff;
}
.bg1 {
    background-color: 0x90000000;
}
.bg2 {
    background-color: 0x90FF0000;
}
.bg3 {
    background-color: 0x9000FF00;
}
.bg4 {
    background-color: 0x900000FF;
}
.bg5 {
    background-color: 0x90FF00FF;
}
.bg6 {
    background-color: 0x90FFFF00;
}
.bg7 {
    background-color: 0x90FF7F00;
}
</style>

<node class="c grow-both horizontal bg1">
    <node class="c grow-both vertical bg2">
        <node class="c grow-h horizontal bg3" value="H1"/>
        <node class="c grow-h horizontal bg4" value="H2"/>
        <node class="c grow-h horizontal bg1" value="H3"/>
    </node>
    <node class="c grow-both vertical bg2">
        <node class="c grow-both horizontal bg4" value="H10"/>
        <node class="c grow-both horizontal bg5" value="H11"/>
        <node class="c grow-both horizontal bg6" value="H12"/>
        <node class="c grow-h horizontal bg2">
            <node class="c grow-h horizontal bg3" value="H10"/>
            <node class="c grow-h horizontal bg4" value="H11"/>
            <node class="c grow-h horizontal bg5" value="H12"/>
        </node>
    </node>
    <node class="c grow-both vertical bg2">
        <node class="c grow-h horizontal bg2">
            <node class="c grow-h horizontal bg3" value="H10"/>
            <node class="c grow-h horizontal bg4" value="H11"/>
            <node class="c grow-h horizontal bg5" value="H12"/>
            <node class="c grow-h horizontal bg6" value="H13"/>
            <node class="c grow-h horizontal bg7" value="H14"/>
        </node>
        <node class="c grow-both horizontal bg1">
            <node class="c grow-both horizontal bg4" value="H7"/>
            <node class="c grow-both horizontal bg5" value="H8"/>
            <node class="c grow-both horizontal bg6" value="H9"/>
        </node>
        <node class="c grow-h horizontal bg2">
            <node class="c grow-h horizontal bg3" value="H10"/>
            <node class="c grow-h horizontal bg4" value="H11"/>
            <node class="c grow-h horizontal bg5" value="H12"/>
            <node class="c grow-h horizontal bg6" value="H13"/>
            <node class="c grow-h horizontal bg7" value="H14"/>
        </node>
    </node>
    <node class="c grow-both vertical bg2">
        <node class="c grow-h horizontal bg3" value="H15"/>
        <node class="c grow-h horizontal bg4" value="H16"/>
        <node class="c grow-both horizontal bg2">
            <node class="c grow-both horizontal bg3" value="H10"/>
            <node class="c grow-both horizontal bg4" value="H11"/>
            <node class="c grow-both horizontal bg5" value="H12"/>
            <node class="c grow-both horizontal bg6" value="H13"/>
            <node class="c grow-both horizontal bg7" value="H14"/>
        </node>
    </node>
</node>
