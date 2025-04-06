<style>
#main {
    flow: vertical;
    gap: 10;
}

.row {
    flow: horizontal;
    gap: 10;
    auto-size: grow fit;
}

.test {
    flow: horizontal;
    auto-size: fit fit;
    background-color: 0xEECACACA;
    gap: 10;
}

.box {
    anchor: middle-left;
    background-color: 0xFF212021;
    
    &.pad-10 { padding: 10; }
    &.mar-10 { margin: 10; }
}
</style>

<node id="main">
    <node class="row">
        <node class="test">
            <node class="box" value="No padding and margin." />
        </node>
        <node class="test">
            <node class="box pad-10" value="Padding 10." />
        </node>
        <node class="test">
            <node class="box mar-10" value="Margin 10." />
        </node>
        <node class="test">
            <node class="box pad-10 mar-10" value="Padding 10 and Margin 10." />
        </node>
    </node>
    <node class="row">
        <node class="test">
            <node class="box" value="MT 10" style={margin: 10 0 0 0;} />
        </node>
        <node class="test">
            <node class="box" value="MR 10" style={margin: 0 10 0 0;} />
        </node>
        <node class="test">
            <node class="box" value="MB 10" style={margin: 0 0 10 0;} />
        </node>
        <node class="test">
            <node class="box" value="ML 10" style={margin: 0 0 0 10;} />
        </node>
        <node class="test">
            <node class="box" value="ML+R 10" style={margin: 0 10 0 10;} />
        </node>
        <node class="test">
            <node class="box" value="MT+B 10" style={margin: 10 0 10 0;} />
        </node>
    </node>
    <node class="row">
        <node class="test">
            <node class="box" value="MT 10 PB 10" style={margin: 10 0 0 0; padding: 0 0 10 0;} />
        </node>
        <node class="test">
            <node class="box" value="MB 10 PT 10" style={margin: 0 0 10 0; padding: 10 0 0 0;} />
        </node>
        <node class="test">
            <node class="box" value="MR 10 PL 10" style={margin: 0 10 0 0; padding: 0 0 0 10;} />
        </node>
        <node class="test">
            <node class="box" value="ML 10 PR 10" style={margin: 0 0 0 10; padding: 0 10 0 0;} />
        </node>
    </node>
    <node class="row">
        <node class="test" style={auto-size: grow fit; margin: 10; padding: 20;}>
            <node style={anchor: middle-center;} value="M10/P20 H-GROW"/>
        </node>
    </node>
    <node class="row" style={size: 0 100;}>
        <node class="test" style={auto-size: fit grow; margin: 10; padding: 20;}>
            <node style={anchor: middle-center;} value="M10/P20 V-GROW"/>
        </node>
    </node>
    <node class="row">
        <node class="test" style={flow: vertical; auto-size: grow grow; margin: 10; padding: 20;}>
            <node style={anchor: middle-center; margin: 5; padding: 5;} value="M10/P20 GROW"/>
            <node style={anchor: middle-center; margin: 5; padding: 5;} value="M10/P20 GROW"/>
            <node style={anchor: middle-center; margin: 5; padding: 5;} value="M10/P20 GROW"/>
            <node style={anchor: middle-center; margin: 5; padding: 5;} value="M10/P20 GROW"/>
            <node style={anchor: middle-center; margin: 5; padding: 5;} value="M10/P20 GROW"/>
        </node>
    </node>
</node>