var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
/// <reference path="../tsDefinitions/phaser.d.ts" />
var IDemon;
(function (IDemon) {
    var Boot = (function (_super) {
        __extends(Boot, _super);
        function Boot() {
            _super.apply(this, arguments);
        }
        Boot.prototype.preload = function () {
            var fileLocation = 'iDemon/';
            this.load.image('preloadBar', fileLocation + 'assets/loader.png');
        };
        Boot.prototype.create = function () {
            // Enable Arcade Physics
            // this.game.physics.enable(this, Phaser.Physics.ARCADE);
            //  Unless you specifically need to support multitouch I would recommend setting this to 1
            this.input.maxPointers = 1;
            //  Phaser will automatically pause if the browser tab the game is in loses focus. You can disable that here:
            // this.stage.disableVisibilityChange = true;
            /*
            if (this.game.device.desktop) {
                //  If you have any desktop specific settings, they can go in here
                // this.stage.scale.pageAlignHorizontally = true;
            }
            else {
                //  Same goes for mobile settings.
            }
            */
            this.game.state.start('Preloader', true, false);
        };
        return Boot;
    }(Phaser.State));
    IDemon.Boot = Boot;
})(IDemon || (IDemon = {}));
/// <reference path="../tsDefinitions/phaser.d.ts" />
var IDemon;
(function (IDemon) {
    var Preloader = (function (_super) {
        __extends(Preloader, _super);
        function Preloader() {
            _super.apply(this, arguments);
        }
        Preloader.prototype.preload = function () {
            var fileLocation = 'iDemon/';
            //  Set-up our preloader sprite
            this.preloadBar = this.add.sprite(200, 250, 'preloadBar');
            this.load.setPreloadSprite(this.preloadBar);
            //  Load our actual games assets
            this.load.image('titleScreen', fileLocation + 'assets/titleScreen.png');
            this.load.image('logo', fileLocation + 'assets/logo.png');
            this.load.audio('music', fileLocation + 'assets/IDemonSong2.ogg', true);
            // this.load.tilemap("stageOneMap", "assets/iDemonStage1.json", null, Phaser.Tilemap.TILED_JSON);
            this.load.tilemap("stageOneMap", fileLocation + "assets/iDemon1-1a.json", null, Phaser.Tilemap.TILED_JSON);
            // this.load.image("brickTiles", "assets/brickTile80.png");
            // this.load.image("stairTiles", "assets/steps.png");
            this.load.image("dungeonTiles", fileLocation + "assets/dungeonTileImagesSheet.png");
            // Player character
            this.load.spritesheet("player", fileLocation + "assets/playerSpriteSheet.png", 55, 140);
            // this.load.image("playerIdle", "assets/playerIdle.png");
            // this.load.image("playerPunching", "assets/playerPunching.png");
            // this.load.image("playerKicking", "assets/playerKick.png");
            // this.load.image("playerCrouching", "assets/playerCrouching.png");
            this.load.image("fist", fileLocation + "assets/fist.png");
            this.load.image("boot", fileLocation + "assets/boot.png");
            // Misc
            this.load.image("cameraBarrier", fileLocation + "assets/cameraBarrier.png");
        };
        Preloader.prototype.create = function () {
            this.game.physics.enable(this, Phaser.Physics.ARCADE);
            if (IDemon.Game.DEBUG_MODE)
                this.game.state.start('Level1', true, false);
            else {
                var tween = this.add.tween(this.preloadBar).to({ alpha: 0 }, 1000, Phaser.Easing.Linear.None, true);
                tween.onComplete.add(this.startMainMenu, this);
            }
        };
        Preloader.prototype.startMainMenu = function () {
            this.game.state.start('MainMenu', true, false);
        };
        return Preloader;
    }(Phaser.State));
    IDemon.Preloader = Preloader;
})(IDemon || (IDemon = {}));
/// <reference path="../tsDefinitions/phaser.d.ts" />
var IDemon;
(function (IDemon) {
    var MainMenu = (function (_super) {
        __extends(MainMenu, _super);
        function MainMenu() {
            _super.apply(this, arguments);
        }
        MainMenu.prototype.create = function () {
            this.background = this.add.sprite(0, 0, 'titleScreen');
            this.background.alpha = 0;
            this.logo = this.add.sprite(this.world.centerX, -300, 'logo');
            this.logo.anchor.setTo(0.5, 0.5);
            this.add.tween(this.background).to({ alpha: 1 }, 2000, Phaser.Easing.Bounce.InOut, true);
            this.add.tween(this.logo).to({ y: 220 }, 2000, Phaser.Easing.Elastic.Out, true, 2000);
            this.input.onDown.addOnce(this.fadeOut, this);
        };
        MainMenu.prototype.fadeOut = function () {
            this.add.tween(this.background).to({ alpha: 0 }, 2000, Phaser.Easing.Linear.None, true);
            var tween = this.add.tween(this.logo).to({ y: 800 }, 2000, Phaser.Easing.Linear.None, true);
            tween.onComplete.add(this.startGame, this);
        };
        MainMenu.prototype.startGame = function () {
            this.game.state.start('Level1', true, false);
        };
        return MainMenu;
    }(Phaser.State));
    IDemon.MainMenu = MainMenu;
})(IDemon || (IDemon = {}));
/// <reference path="../tsDefinitions/phaser.d.ts" />
var IDemon;
(function (IDemon) {
    (function (PlayerState) {
        PlayerState[PlayerState["Standing"] = 0] = "Standing";
        PlayerState[PlayerState["Crouching"] = 1] = "Crouching";
        PlayerState[PlayerState["Airborne"] = 2] = "Airborne";
        PlayerState[PlayerState["Dead"] = 3] = "Dead";
    })(IDemon.PlayerState || (IDemon.PlayerState = {}));
    var PlayerState = IDemon.PlayerState;
    ;
    var Player = (function (_super) {
        __extends(Player, _super);
        function Player(game, x, y) {
            _super.call(this, game, x, y, "player", 0);
            this.playerHasControl = true;
            // Constants
            this.PLAYER_WALK_SPEED = 400;
            this.PLAYER_CROUCH_WALK_SPEED = 140;
            // Enable Player's Physics Body
            this.game.physics.arcade.enableBody(this);
            this.anchor.setTo(.5, .5);
            this.body.gravity.y = 2000;
            this.playerState = PlayerState.Airborne;
            this.checkWorldBounds = true;
            this.events.onOutOfBounds.add(this.killPlayer, this);
            // this.game.camera.follow(this.player);
            game.add.existing(this);
            // this.game = game;
            // old create() code
            this.playerAttackSprites = this.game.add.group(this, "playerAttackSprites");
            this.playerAttackSprites.enableBody = true;
            // this.cursorKeys = this.game.input.keyboard.createCursorKeys();
            //  Create 3 hotkeys, C=Punch, V=Kick, Space=Jump
            this.keySpace = this.game.input.keyboard.addKey(Phaser.Keyboard.SPACEBAR);
            this.keySpace.onDown.add(this.jump, this);
            this.keyC = this.game.input.keyboard.addKey(Phaser.Keyboard.C);
            this.keyC.onDown.add(this.playerPunch, this);
            this.keyV = this.game.input.keyboard.addKey(Phaser.Keyboard.V);
            this.keyV.onDown.add(this.playerKick, this);
            // Player limbs
            // this.playerFist = this.playerAttackSprites.create(-1000, -1000, "fist");
            this.playerFist = this.playerAttackSprites.create(this.width / 1.6, -(this.height / 4 + 5), "fist");
            this.playerFist.anchor.setTo(.5, .5);
            this.playerFist.kill();
            // this.playerBoot = this.playerAttackSprites.create(-1000, -1000, "boot");
            this.playerBoot = this.playerAttackSprites.create(this.width / 1.6, this.height / 4, "boot");
            this.playerBoot.anchor.setTo(.5, .5);
            this.playerBoot.kill();
        }
        Player.prototype.create = function () {
        };
        Player.prototype.update = function () {
            this.body.velocity.x = 0;
            if (this.playerState != PlayerState.Dead) {
                // Handle Inputs
                // if (this.cursorKeys.left.isDown && this.playerHasControl)
                if (this.game.input.keyboard.isDown(Phaser.Keyboard.LEFT) && this.playerHasControl) {
                    this.body.velocity.x = this.playerState == PlayerState.Crouching ? -this.PLAYER_CROUCH_WALK_SPEED : -this.PLAYER_WALK_SPEED;
                    // this.player.animations.play('left');
                    if (this.scale.x == 1) {
                        this.scale.x = -1;
                    }
                }
                else if (this.game.input.keyboard.isDown(Phaser.Keyboard.RIGHT) && this.playerHasControl && !this.body.blocked.right) {
                    this.body.velocity.x = this.playerState == PlayerState.Crouching ? this.PLAYER_CROUCH_WALK_SPEED : this.PLAYER_WALK_SPEED;
                    if (this.scale.x == -1) {
                        this.scale.x = 1;
                        this.playerFist.scale.x = 1;
                    }
                }
                else if (this.game.input.keyboard.isDown(Phaser.Keyboard.DOWN) && this.playerHasControl) {
                    if (this.playerState == PlayerState.Standing) {
                        this.playerState = PlayerState.Crouching;
                        this.body.setSize(55, 80, 0, 30);
                        // this.loadTexture("playerCrouching", 0, false);
                        this.animations.frame = 1;
                    }
                }
                else {
                    // No Keys Pressed
                    // If the player is crouching we can now stand them up
                    if (this.playerState == PlayerState.Crouching) {
                        this.goBackToIdle();
                    }
                }
                // If Airborne, check to see if they've reached the ground
                if (this.playerState == PlayerState.Airborne) {
                    if (this.body.blocked.down) {
                        this.goBackToIdle();
                    }
                }
                else {
                    if (!this.body.blocked.down) {
                        this.playerState = PlayerState.Airborne;
                    }
                }
            } // /if (this.playerState != PlayerState.Dead)
        }; // /update()
        // Public Player Methods
        Player.prototype.killPlayer = function () {
            var _this = this;
            this.playerState = PlayerState.Dead;
            this.playerHasControl = false;
            this.scale.y = -1;
            this.body.velocity.y = -1040;
            this.checkWorldBounds = false;
            this.game.time.events.add(Phaser.Timer.SECOND * 5, (function () { _this.destroy(); }), this);
        };
        Player.prototype.jump = function () {
            if (this.playerState == PlayerState.Standing && this.playerHasControl) {
                this.body.velocity.y = -835;
                this.playerState = PlayerState.Airborne;
            }
        };
        Player.prototype.playerPunch = function () {
            if (this.playerState == PlayerState.Standing && this.playerHasControl) {
                this.body.velocity.x = 0;
                this.playerHasControl = false;
                // TODO: Replace this with a spritesheet frame
                // this.loadTexture("playerPunching", 0, false);
                this.animations.frame = 3;
                this.game.time.events.add(400, this.goBackToIdle, this);
                // this.playerFist.x = this.x + (this.width / 1.6);
                // this.playerFist.y = this.y - this.height / 4 - 5;
                this.playerFist.revive();
            }
        };
        Player.prototype.playerKick = function () {
            if (this.playerState == PlayerState.Standing && this.playerHasControl) {
                this.body.velocity.x = 0;
                this.playerHasControl = false;
                // TODO: Replace this with a spritesheet frame
                // this.loadTexture("playerKicking", 0, false);
                this.animations.frame = 2;
                this.game.time.events.add(400, this.goBackToIdle, this);
                // this.playerBoot.x = this.x + (this.width / 1.6);
                // this.playerBoot.y = this.y + this.height / 4;
                this.playerBoot.revive();
            }
        };
        // Private Player Methods
        Player.prototype.goBackToIdle = function () {
            if (this.playerState != PlayerState.Dead) {
                this.body.setSize(55, 140, 0, 0);
                // this.loadTexture("playerIdle", 0, false);
                this.animations.frame = 0;
                this.playerState = PlayerState.Standing;
                this.playerFist.kill();
                this.playerBoot.kill();
                this.playerHasControl = true;
            }
        };
        return Player;
    }(Phaser.Sprite));
    IDemon.Player = Player; // /class Player
})(IDemon || (IDemon = {})); // /module IDemon
/// <reference path="../tsDefinitions/phaser.d.ts" />
/// <reference path="Player.ts" />
var IDemon;
(function (IDemon) {
    var Level1 = (function (_super) {
        __extends(Level1, _super);
        function Level1() {
            _super.apply(this, arguments);
            // checkCameraBarrierCollision:Function;
            // checkTilemapCollision:Function;
            this.gameScrollSpeed = 2;
            this.floorYmax = 0;
        }
        Level1.prototype.create = function () {
            if (IDemon.Game.MUSIC_ON) {
                this.music = this.add.audio('music', 1, false);
                this.music.play();
            }
            // DEBUGGING
            this.floor = new Phaser.Rectangle(0, 550, 800, 50);
            this.game.stage.backgroundColor = 0x000000;
            /*            this.stageOneMap = this.game.add.tilemap("stageOneMap", 80, 80, 70, 8);
                        this.stageOneMap.addTilesetImage("bricks", "brickTiles");
                        this.stageOneMap.addTilesetImage("stairs", "stairTiles");*/
            this.stageOneMap = this.game.add.tilemap("stageOneMap", 40, 40, 90, 16);
            this.stageOneMap.addTilesetImage("dungeonTileImagesSheet", "dungeonTiles");
            this.brickLayer = this.stageOneMap.createLayer("Bricks");
            this.stageOneMap.createLayer("Stairs");
            this.brickLayer.resizeWorld();
            // Setup Camera
            // this.game.camera.x = this.stageOneMap.layers[0].widthInPixels / 2;
            this.game.camera.x = 0;
            this.game.camera.y = 0;
            // Setup the bricks for collisions
            this.stageOneMap.setCollision([6, 7, 18, 19], true, this.brickLayer.index, true);
            // Add Camera Barriers
            this.cameraBarriers = this.game.add.group();
            this.cameraBarriers.enableBody = true;
            // this.cameraBarriers.create(0, 0, "cameraBarrier").fixedToCamera = true;
            this.cameraBarriers.create(0, 0, "cameraBarrier");
            // this.cameraBarriers.create(790, 0, "cameraBarrier").fixedToCamera = true;
            this.cameraBarriers.create(790, 0, "cameraBarrier");
            this.cameraBarriers.setAll("body.immovable", true);
            // Add Player to Game
            this.player = new IDemon.Player(this.game, 200, 300);
            // TO DO: Enable Later!
            // this.enemies = this.game.add.group();
            // this.enemies.enableBody = true;
            // Phaser.Physics.Arcade.TILE_BIAS = 40;
        };
        Level1.prototype.update = function () {
            var _this = this;
            // DEBUGGING
            this.floor.x = this.player.x + Math.abs(this.player.width / 2);
            this.floor.y = this.player.y;
            this.floor.height = 3;
            this.floor.width = 3;
            if (this.player.playerState != IDemon.PlayerState.Dead) {
                // Moving the camera barriers
                if (this.game.camera.view.x < this.game.world.width - this.stage.width) {
                    this.cameraBarriers.forEach(function (bar) { bar.body.x += _this.gameScrollSpeed; }, this);
                }
                // this.player.body.moves = true; // Nope
                // This is the autoscrolling behavior
                this.game.camera.x += this.gameScrollSpeed;
                // Phaser.Physics.Arcade.TILE_BIAS = 40;
                this.game.physics.arcade.TILE_BIAS = 40;
                this.collBetweenBrickAndPlayer = this.game.physics.arcade.collide(this.player, this.brickLayer);
                this.game.physics.arcade.collide(this.player, this.cameraBarriers, this.checkCameraBarrierCollision, null, this);
            }
        };
        Level1.prototype.checkCameraBarrierCollision = function () {
            /*
            if (this.player.body.blocked.right) {
                // Played got squished and died
                this.killPlayer();
            }
            */
            var tilesTouching = this.brickLayer.getTiles(this.player.x + Math.abs(this.player.width / 2), this.player.y, 3, 3, true);
            if (tilesTouching.length > 0) {
                this.player.killPlayer();
            }
            else {
                // Give the player a chance to get off the left barrier
                if (this.game.input.keyboard.isDown(Phaser.Keyboard.RIGHT) && this.player.playerHasControl) {
                    this.player.body.velocity.x = this.player.playerState == IDemon.PlayerState.Crouching ? this.player.PLAYER_CROUCH_WALK_SPEED : this.player.PLAYER_WALK_SPEED;
                }
            }
        }; // /checkCameraBarrierCollision()
        Level1.prototype.render = function () {
            this.game.debug.cameraInfo(this.game.camera, 500, 32);
            if (this.player.playerState != IDemon.PlayerState.Dead) {
                this.game.debug.spriteInfo(this.player, 32, 32);
            }
            // this.game.debug.spriteInfo(this.player.playerFist, 500, 32);
            // this.game.debug.spriteInfo(this.player.playerBoot, 500, 132);
            this.game.debug.body(this.player);
            // this.game.debug.body(this.playerHalo);
            // this.game.debug.body(this.brickLayer);
            // this.game.debug.body(this.playerFist);
            // this.game.debug.text('Fist X: ' + this.playerFist.x + ', Fist Y: ' + this.playerFist.y, 10, 120);
            this.game.debug.text('playerState: ' + IDemon.PlayerState[this.player.playerState], 10, 120);
            // this.game.debug.text('Blocked Right: ' + this.player.body.blocked.right, 290, 120);
            this.game.debug.text('PlayerBrickColl: ' + this.collBetweenBrickAndPlayer, 240, 120);
            // this.game.debug.text('Blocked Bottom: ' + this.player.body.blocked.down, 490, 120);
            // this.game.debug.text('Halo Overlaps BrickLayer: ' + this.game.physics.arcade.overlap(this.playerHalo, this.brickLayer), 490, 120);
            // this.game.debug.text('PlayerX / Player.width/2: ' + this.player.x + '/' + this.player.width / 2, 350, 120);
            // this.game.debug.text('Touching Right: ' + this.player.body.touching.right, 10, 170);
            /*this.game.debug.text('Halo Blocked Right: ' + this.playerHalo.body.blocked.right, 10, 170);*/
            this.game.debug.geom(this.floor, '#0fffff');
        };
        return Level1;
    }(Phaser.State));
    IDemon.Level1 = Level1; // /class Level1
})(IDemon || (IDemon = {})); // /module IDemon 
/// <reference path="Boot.ts" />
/// <reference path="Preloader.ts" />
/// <reference path="MainMenu.ts" />
/// <reference path="Level1.ts" />
var IDemon;
(function (IDemon) {
    var Game = (function (_super) {
        __extends(Game, _super);
        function Game() {
            _super.call(this, 800, 600, Phaser.AUTO, 'content', null);
            this.state.add('Boot', IDemon.Boot, false);
            this.state.add('Preloader', IDemon.Preloader, false);
            this.state.add('MainMenu', IDemon.MainMenu, false);
            this.state.add('Level1', IDemon.Level1, false);
            this.state.start('Boot');
        }
        Object.defineProperty(Game, "DEBUG_MODE", {
            // Game-wide Constants
            // Class members cannot have the const keyword.   -bep 2016 4 10 
            // public const DEBUG_MODE:boolean = false;
            get: function () { return true; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Game, "MUSIC_ON", {
            get: function () { return false; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Game, "SFX_ON", {
            get: function () { return true; },
            enumerable: true,
            configurable: true
        });
        return Game;
    }(Phaser.Game));
    IDemon.Game = Game;
})(IDemon || (IDemon = {}));
// when the page has finished loading, create our game
window.onload = function () {
    // game = new IDemon();
    var game = new IDemon.Game();
};
//# sourceMappingURL=game.js.map