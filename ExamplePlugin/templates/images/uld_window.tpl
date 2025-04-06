<style>
#main {
    flow: vertical;
    border-radius: 10;
      
    .header {
        flow: horizontal;
        size: 0 50;
        auto-size: grow fit;
    }
    
    .content {
        flow: horizontal;
        auto-size: grow grow;
        
        .uld.mc {
            auto-size: grow grow;
            
            .image {
                size: 0 0;
                auto-size: grow grow;
                image-scale-mode: original;
                image-tile-mode: decal;
                image-rotation: 90;
                image-scale: 1.0;
                image-offset: 100 100;
                image-blur: 0 4;
                drop-shadow: 8 8 16 16;
               
                icon-id: 113; // 121041;
                padding: 10;
            }
        }
    }
    
    .footer {
        auto-size: grow fit;
    }
}

.title {
    padding: 10 0;
    font-size: 14;
    color: 0xFFAFEFFF;
    outline-color: 0xFF000000;
    outline-size: 1;
}

.uld {
    uld-resource: "ui/uld/TeleportTown";
    uld-parts-id: 2;
    uld-style: light;
    image-scale-mode: original;
    image-tile-mode: repeat;
    image-scale: 0.5;
    
    &.tl {
        uld-part-id: 0;
        size: 12 50;
    }
    &.tc {
        uld-part-id: 1;
        size: 0 50;
        auto-size: grow fit;
    }
    &.tr {
        uld-part-id: 2;
        size: 16 50;
    }
    &.ml {
        uld-part-id: 3;
        size: 12 0;
        auto-size: fit grow;
    }
    &.mc {
        uld-part-id: 4;
        size: 0 0;
        auto-size: grow grow;
    }
    &.mr {
        uld-part-id: 5;
        size: 16 0;
        auto-size: fit grow;
    }
    &.bl {
        uld-part-id: 6;
        size: 12 24;
    }
    &.bc {
        uld-part-id: 7;
        size: 0 24;
        auto-size: grow fit;
    }
    &.br {
        uld-part-id: 8;
        size: 16 24;
    }
}
</style>

<node id="main">
    <node class="header">
        <node class="uld tl" />
        <node class="uld tc">
            <node class="title" value="A Window made up of ULD textures!"/>
        </node>
        <node class="uld tr" />
    </node>
    <node class="content">
        <node class="uld ml" />
        <node class="uld mc">
            <node class="image" />
        </node>
        <node class="uld mr" />
    </node>
    <node class="footer">
        <node class="uld bl" />
        <node class="uld bc" />
        <node class="uld br" />
    </node>
</node>