var Shapeshifter;
(function (Shapeshifter) {
    class BatType {
        constructor(batTypeID, name, health, bodyTint, bulletTint, scale, behaviorOnSpawn) {
            this.batTypeID = batTypeID;
            this.name = name;
            this.health = health;
            this.bodyTint = bodyTint;
            this.bulletTint = bulletTint;
            this.scale = scale;
            this.behaviorOnSpawn = behaviorOnSpawn;
        }
    }
    Shapeshifter.BatType = BatType;
    function brownBatBehavior(bat) {
        bat.body.velocity.y = 200;
    }
    function blueBatBehavior(bat) {
        let tweenDown = bat.game.add.tween(bat).to({ y: 150 }, 1500, Phaser.Easing.Quadratic.Out);
        tweenDown.onComplete.addOnce(bat.shootStraightDown, bat);
        let tweenUp = bat.game.add.tween(bat).to({ y: -100 }, 1500, Phaser.Easing.Quadratic.In);
        tweenUp.onComplete.addOnce(() => bat.kill(), bat);
        tweenDown.chain(tweenUp);
        tweenDown.start();
    }
    function orangeBatBehavior(bat) {
        bat.body.velocity.y = 120;
        bat.game.time.events.repeat(800, 4, () => {
            bat.shootAtPlayer();
        });
    }
    function redBatBehavior(bat) {
        function redShooting() {
            bat.game.time.events.repeat(750, bat.game.rnd.integerInRange(3, 6), () => bat.shootSpreadAtPlayer());
        }
        let tweenDown = bat.game.add.tween(bat).to({ y: 100 }, 1500, Phaser.Easing.Quadratic.Out);
        tweenDown.onComplete.addOnce(() => redShooting());
        let tweenUp = bat.game.add.tween(bat).to({ y: -100 }, 1500, Phaser.Easing.Quadratic.In);
        tweenUp.onComplete.addOnce(() => bat.kill(), bat);
        tweenDown.start();
        bat.game.time.events.add(4500, () => tweenUp.start(), this);
    }
    Shapeshifter.BatTypes = [
        new BatType(0, "BrownBat", 20, 0x2B1D10, 0x2B1D10, 1, brownBatBehavior),
        new BatType(1, "BlueBat", 40, 0x2BCFF4, 0x2BCFF4, 1, blueBatBehavior),
        new BatType(2, "OrangeBat", 50, 0xF6AB26, 0xF6AB26, 1, orangeBatBehavior),
        new BatType(3, "RedBat", 150, 0xED1C31, 0xED1C31, 3, redBatBehavior)
    ];
    class Bat extends Phaser.Sprite {
        constructor(game, x, y, enemyBulletPool, player) {
            super(game, x, y, 'bat', 0);
            this.enemyBulletPool = enemyBulletPool;
            this.player = player;
            this.game.physics.arcade.enableBody(this);
            this.body.collideWorldBounds = false;
            this.anchor.setTo(0.5, 0);
            this.animations.add('fly', [0, 1], 5, true);
            game.add.existing(this);
            this.alive = false;
            this.exists = false;
            this.visible = false;
        }
        update() {
            if (this.y > Shapeshifter.Game.GAME_HEIGHT + 200) {
                this.kill();
                return;
            }
        }
        reviveBat(batTypeName) {
            this.batType = Shapeshifter.BatTypes.find((bt) => bt.name == batTypeName);
            if (!this.batType)
                console.log("Could not find BatType with batTypeName == " + batTypeName);
            super.revive(this.batType.health);
            this.scale.x = this.batType.scale;
            this.scale.y = this.batType.scale;
            this.maxHealth = this.batType.health;
            this.isDamaged = false;
            this.tint = this.batType.bodyTint;
            this.x = this.game.rnd.between(40, Shapeshifter.Game.WORLD_WIDTH - 40);
            this.y = -50;
            this.animations.play('fly');
            this.batType.behaviorOnSpawn(this);
        }
        shootStraightDown() {
            if (!this.alive)
                return;
            let bullet = this.enemyBulletPool.getFirstExists(false);
            bullet.tint = this.tint;
            bullet.reset(this.x, this.y - 20);
            bullet.body.velocity.y = 500;
        }
        shootAtPlayer() {
            if (!this.alive)
                return;
            let bullet = this.enemyBulletPool.getFirstExists(false);
            bullet.tint = this.tint;
            bullet.reset(this.x, this.y - 20);
            let shotAngle = this.game.physics.arcade.angleBetween(this, this.player);
            this.game.physics.arcade.velocityFromAngle(Phaser.Math.radToDeg(shotAngle), 300, bullet.body.velocity);
        }
        shootSpreadAtPlayer() {
            if (!this.alive)
                return;
            let deadBulletsAvailable = this.enemyBulletPool.countDead();
            if (deadBulletsAvailable < 5) {
                console.log("Not enough bullets available for spread shot:" + deadBulletsAvailable);
                return;
            }
            var initialAngle = Phaser.Math.radToDeg(this.game.physics.arcade.angleBetween(this, this.player)) - 60;
            for (let i = 0; i < 5; i++) {
                let bullet = this.enemyBulletPool.getFirstExists(false);
                bullet.reset(this.x, this.y - 20);
                bullet.tint = this.tint;
                let shotAngle = initialAngle + ((i + 1) * 20);
                this.game.physics.arcade.velocityFromAngle(shotAngle, 320, bullet.body.velocity);
            }
        }
        static flyStraightDown(enemy) {
            enemy.body.velocity.y = 200;
        }
    }
    Shapeshifter.Bat = Bat;
})(Shapeshifter || (Shapeshifter = {}));
var Shapeshifter;
(function (Shapeshifter) {
    class Boot extends Phaser.State {
        preload() {
            let fileLocation = 'shapeshifter/';
            this.load.image('preloadBar', fileLocation + 'assets/loader.png');
        }
        create() {
            this.game.physics.enable(this, Phaser.Physics.ARCADE);
            this.input.maxPointers = 1;
            this.stage.disableVisibilityChange = true;
            if (this.game.device.desktop) {
            }
            else {
            }
            this.game.state.start('Preloader', true, false);
        }
    }
    Shapeshifter.Boot = Boot;
})(Shapeshifter || (Shapeshifter = {}));
var Shapeshifter;
(function (Shapeshifter) {
    class Preloader extends Phaser.State {
        preload() {
            let fileLocation = 'shapeshifter/';
            this.preloadBar = this.add.sprite(200, 250, 'preloadBar');
            this.load.setPreloadSprite(this.preloadBar, 0);
            this.load.image('titlepage', fileLocation + 'assets/titlepage.jpg');
            this.load.image('logo', fileLocation + 'assets/logo.png');
            this.load.audio('music', fileLocation + 'assets/title.mp3', true);
            this.load.spritesheet('rabbit', fileLocation + 'assets/rabbitSpriteSheet.png', 40, 40, 6);
            this.load.spritesheet('bat', fileLocation + 'assets/batSpriteSheet2.png', 60, 40, 2);
            this.load.spritesheet('wizardSpriteSheet', fileLocation + 'assets/wizardSpriteSheet.png', 28, 60, 5);
            this.load.image('level1ground', fileLocation + 'assets/caveFloorTile.png');
            this.load.image('healthBar', fileLocation + 'assets/healthBar.png');
            this.load.image('wizardBubble', fileLocation + 'assets/wizardBubble.png');
            this.load.image('crowBubble', fileLocation + 'assets/wizardBubble.png');
            this.load.image('wizardBullet', fileLocation + 'assets/wizardBullet.png');
            this.load.image('batBullet', fileLocation + 'assets/batBullet.png');
            this.load.audio('mobDying', fileLocation + 'assets/SFX/mobDying.ogg', true);
            this.load.audio('playerDying', fileLocation + 'assets/SFX/playerDying.ogg', true);
            this.load.audio('playerHurt', fileLocation + 'assets/SFX/playerHurt.ogg', true);
            this.load.audio('rabbitJump', fileLocation + 'assets/SFX/rabbitJump.ogg', true);
            this.load.audio('transform', fileLocation + 'assets/SFX/transform.ogg', true);
            this.load.audio('wizardShooting', fileLocation + 'assets/SFX/wizardShooting.ogg', true);
            this.load.audio('wizardShootingSubdued', fileLocation + 'assets/SFX/wizardShootingSubdued.ogg', true);
            this.load.audio('ssLevel1Theme', fileLocation + 'assets/ssLevel1Theme.ogg', true);
            this.load.tilemap("caveStageOneMap", fileLocation + "assets/stages/firstCave2.json", null, Phaser.Tilemap.TILED_JSON);
            this.load.image("caveTiles", fileLocation + "assets/caveTileSet1.png");
        }
        create() {
            if (Shapeshifter.Game.DEBUG_MODE)
                this.game.state.start('Level1', true, false);
            else {
                this.startMainMenu();
            }
        }
        startMainMenu() {
            this.game.state.start('MainMenu', true, false);
        }
    }
    Shapeshifter.Preloader = Preloader;
})(Shapeshifter || (Shapeshifter = {}));
var Shapeshifter;
(function (Shapeshifter) {
    class MainMenu extends Phaser.State {
        create() {
            let textStyle = { font: "20px Arial", fill: "#ff0000", align: "center" };
            let gameOverText = `Shapeshifter
      Move with arrow keys
      1 and 2 are used for shapeshifting (if you have gotten the ability)
      Wizard: Press Q to shoot
      Click anywhere to start the game`;
            let text = this.game.add.text(0, 0, gameOverText, textStyle);
            this.input.onDown.addOnce(() => this.game.state.start('Level1', true, false), this);
        }
        startGame() {
            this.game.state.start('Level1', true, false);
        }
    }
    Shapeshifter.MainMenu = MainMenu;
})(Shapeshifter || (Shapeshifter = {}));
var Shapeshifter;
(function (Shapeshifter) {
    (function (PlayerState) {
        PlayerState[PlayerState["Grounded"] = 0] = "Grounded";
        PlayerState[PlayerState["Airborne"] = 1] = "Airborne";
        PlayerState[PlayerState["Dead"] = 2] = "Dead";
        PlayerState[PlayerState["Transforming"] = 3] = "Transforming";
    })(Shapeshifter.PlayerState || (Shapeshifter.PlayerState = {}));
    var PlayerState = Shapeshifter.PlayerState;
    ;
    class PlayerForm {
        constructor(playerFormID, name, movementSpeed, walkSidewaysAnimationKeyName, walkDownAnimationKeyName, walkUpAnimationKeyName, abilityOne, abilityTwo, abilityThree) {
            this.playerFormID = playerFormID;
            this.name = name;
            this.movementSpeed = movementSpeed;
            this.walkSidewaysAnimationKeyName = walkSidewaysAnimationKeyName;
            this.walkDownAnimationKeyName = walkDownAnimationKeyName;
            this.walkUpAnimationKeyName = walkUpAnimationKeyName;
            this.abilityOne = abilityOne;
            this.abilityTwo = abilityTwo;
            this.abilityThree = abilityThree;
        }
    }
    Shapeshifter.PlayerForm = PlayerForm;
    Shapeshifter.PlayerForms = [
        new PlayerForm(0, "Rabbit", 250, "rabbitWalkSideways", "rabbitWalkDown", "rabbitWalkUp", null, null, null),
        new PlayerForm(1, "Wizard", 150, "wizardWalk", "wizardWalk", "wizardWalk", null, null, null),
        new PlayerForm(2, "Crow", 200, "wizardWalk", "wizardWalk", "wizardWalk", null, null, null)
    ];
    class Player extends Phaser.Sprite {
        constructor(game, x, y) {
            super(game, x, y, 'rabbit', 0);
            this.game.physics.arcade.enableBody(this);
            this.body.collideWorldBounds = true;
            this.anchor.setTo(.5, .5);
            this.animations.add('rabbitWalkSideways', [0, 1], 5, true);
            this.animations.add('rabbitWalkDown', [2, 3], 5, true);
            this.animations.add('rabbitWalkUp', [4, 5], 5, true);
            this.animations.add('wizardWalk', [0, 1, 2], 5, true);
            this.animations.add('wizardWalkAndShoot', [3, 4], 5, true);
            game.add.existing(this);
            this.maxHealth = 100;
            this.health = this.maxHealth;
            this.playerState = PlayerState.Grounded;
            this.healthBar = this.game.add.sprite(20, 20, 'healthBar');
            this.healthBar.anchor.setTo(0, 1);
            this.healthBar.scale.setTo(1, 0.5);
            this.healthBar.fixedToCamera = true;
            this.hasWizardForm = false;
            this.hasCrowForm = false;
            this.playerForm = Shapeshifter.PlayerForms.find((pf) => pf.name == "Rabbit");
            this.playerDyingSound = this.game.add.audio('playerDying');
            this.transformationSound = this.game.add.audio('transform');
            this.wizardShootingSound = this.game.add.audio('wizardShootingSubdued');
            this.takeDamageCooldown = 0;
            this.transformationCooldown = 0;
            this.wizardShootingCooldown = 0;
            this.keyQ = this.game.input.keyboard.addKey(Phaser.Keyboard.C);
            this.keyW = this.game.input.keyboard.addKey(Phaser.Keyboard.W);
            this.keyEnter = this.game.input.keyboard.addKey(Phaser.Keyboard.ENTER);
            this.playerWeapon = game.add.weapon(100, "wizardBullet");
            this.playerWeapon.bulletKillType = Phaser.Weapon.KILL_WORLD_BOUNDS;
            this.playerWeapon.bulletAngleOffset = 90;
            this.playerWeapon.bulletSpeed = 500;
            this.playerWeapon.fireRate = 200;
            this.playerWeapon.trackSprite(this, 14, 0);
            this.playerWeapon.onFire.add(() => this.wizardShootingSound.play());
        }
        update() {
            if (this.playerState != PlayerState.Transforming) {
                this.body.velocity.x = 0;
                this.body.velocity.y = 0;
            }
            if (this.takeDamageCooldown >= 1)
                this.takeDamageCooldown--;
            if (this.transformationCooldown >= 1)
                this.transformationCooldown--;
            if (this.wizardShootingCooldown >= 1)
                this.wizardShootingCooldown--;
            this.handleKeys();
        }
        handleKeys() {
            switch (this.playerState) {
                case PlayerState.Grounded:
                    if (this.game.input.keyboard.isDown(Phaser.Keyboard.LEFT)) {
                        this.body.velocity.x = -this.playerForm.movementSpeed;
                        this.animations.play(this.playerForm.walkSidewaysAnimationKeyName);
                        if (this.scale.x == 1) {
                            this.scale.x = -1;
                        }
                    }
                    else if (this.game.input.keyboard.isDown(Phaser.Keyboard.RIGHT)) {
                        this.body.velocity.x = this.playerForm.movementSpeed;
                        this.animations.play(this.playerForm.walkSidewaysAnimationKeyName);
                        if (this.scale.x == -1) {
                            this.scale.x = 1;
                        }
                    }
                    if (this.game.input.keyboard.isDown(Phaser.Keyboard.DOWN)) {
                        this.body.velocity.y = this.playerForm.movementSpeed;
                        if (this.body.velocity.x == 0) {
                            this.animations.play(this.playerForm.walkDownAnimationKeyName);
                        }
                    }
                    else if (this.game.input.keyboard.isDown(Phaser.Keyboard.UP)) {
                        this.body.velocity.y = -this.playerForm.movementSpeed;
                        if (this.body.velocity.x == 0) {
                            this.animations.play(this.playerForm.walkUpAnimationKeyName);
                        }
                    }
                    if (this.game.input.keyboard.isDown(Phaser.Keyboard.Q)) {
                        if (this.playerForm.name == "Wizard") {
                            this.animations.play('wizardWalkAndShoot');
                            if (this.transformationCooldown < 1) {
                                this.playerWeapon.fire();
                            }
                        }
                    }
                    if (this.game.input.keyboard.isDown(Phaser.Keyboard.TWO)) {
                        if (this.hasWizardForm && this.playerForm.name != "Wizard" && (this.transformationCooldown < 1)) {
                            this.transformationSound.play();
                            this.playerState = PlayerState.Transforming;
                            this.transformationCooldown = 60;
                            this.body.velocity.x = 0;
                            this.body.velocity.y = Shapeshifter.Game.VELOCITY_TO_MATCH_SCROLL_SPEED;
                            this.loadTexture("wizardSpriteSheet", 0, true);
                            this.playerForm = Shapeshifter.PlayerForms.find((pf) => pf.name == "Wizard");
                            this.playerState = PlayerState.Grounded;
                            this.animations.play('wizardWalk');
                        }
                    }
                    if (this.game.input.keyboard.isDown(Phaser.Keyboard.ONE)) {
                        if (this.playerForm.name != "Rabbit" && this.transformationCooldown < 1) {
                            this.transformationSound.play();
                            this.playerState = PlayerState.Transforming;
                            this.transformationCooldown = 60;
                            this.body.velocity.x = 0;
                            this.body.velocity.y = Shapeshifter.Game.VELOCITY_TO_MATCH_SCROLL_SPEED;
                            this.loadTexture("rabbit", 0, true);
                            this.playerForm = Shapeshifter.PlayerForms.find((pf) => pf.name == "Rabbit");
                            this.playerState = PlayerState.Grounded;
                            this.animations.play("rabbitWalkUp");
                        }
                    }
                    if (this.body.velocity.x == 0 && this.body.velocity.y == 0) {
                        this.animations.play(this.playerForm.walkUpAnimationKeyName);
                    }
                    break;
                case PlayerState.Dead:
                    this.keyEnter.onDown.add(() => {
                        this.game.sound.stopAll();
                        this.game.state.start('Level1', true, false);
                    });
                    break;
            }
        }
        takeDamage(damageAmount) {
            this.takeDamageCooldown = 60;
            this.health -= damageAmount;
            this.healthBar.scale.setTo(this.health / 100, 0.5);
            if (this.health <= 0) {
                this.die();
            }
        }
        die() {
            this.healthBar.visible = false;
            this.playerDyingSound.play();
            this.playerState = PlayerState.Dead;
            this.kill();
            let textStyle = { font: "32px Arial", fill: "#ff0000", align: "center" };
            let gameOverText = `GAME OVER
      PRESS ENTER TO RESTART`;
            let text = this.game.add.text(this.game.camera.x + 30, this.game.world.centerY - 40, gameOverText, textStyle);
        }
    }
    Shapeshifter.Player = Player;
})(Shapeshifter || (Shapeshifter = {}));
var Shapeshifter;
(function (Shapeshifter) {
    (function (PowerUpType) {
        PowerUpType[PowerUpType["Wizard"] = 0] = "Wizard";
        PowerUpType[PowerUpType["Crow"] = 1] = "Crow";
    })(Shapeshifter.PowerUpType || (Shapeshifter.PowerUpType = {}));
    var PowerUpType = Shapeshifter.PowerUpType;
    ;
    class PowerUp extends Phaser.Sprite {
        constructor(game, type) {
            let keyToUse = 'wizardBubble';
            if (type == PowerUpType.Crow)
                keyToUse = 'crowBubble';
            super(game, Shapeshifter.Game.WORLD_WIDTH / 2, -100, keyToUse, 0);
            this.powerUpType = type;
            this.game.physics.arcade.enableBody(this);
            this.anchor.setTo(0.5, 0);
            game.add.existing(this);
            this.body.velocity.y = Shapeshifter.Game.VELOCITY_TO_MATCH_SCROLL_SPEED;
        }
    }
    Shapeshifter.PowerUp = PowerUp;
})(Shapeshifter || (Shapeshifter = {}));
var Shapeshifter;
(function (Shapeshifter) {
    class Level1 extends Phaser.State {
        create() {
            this.game.world.setBounds(0, 0, Shapeshifter.Game.WORLD_WIDTH, Shapeshifter.Game.WORLD_HEIGHT);
            this.background = this.add.tileSprite(0, 0, Shapeshifter.Game.WORLD_WIDTH, Shapeshifter.Game.WORLD_HEIGHT, 'level1ground');
            this.ssLevel1Theme = this.add.audio('ssLevel1Theme', 0.3, false);
            this.ssLevel1Theme.play();
            this.getPowerUpSound = this.game.add.audio('wizardShooting');
            this.playerHurtSound = this.game.add.audio('playerHurt');
            this.enemyDyingSound = this.game.add.audio('mobDying');
            if (Shapeshifter.Game.MUTE_SOUND)
                this.game.sound.mute = true;
            this.stageOneMap = this.game.add.tilemap("caveStageOneMap", 40, 40, 18, 72);
            this.stageOneMap.addTilesetImage("caveTileSet1", "caveTiles");
            this.debrisLayer = this.stageOneMap.createLayer("Debris", 720, 2880);
            this.debrisLayer.fixedToCamera = false;
            this.debrisLayer.outOfBoundsKill = false;
            this.debrisLayer.y = -2000;
            this.player = new Shapeshifter.Player(this.game, Shapeshifter.Game.WORLD_WIDTH / 2, Shapeshifter.Game.WORLD_HEIGHT - 20);
            this.powerUps = this.game.add.group();
            this.createEnemyBulletPool();
            this.enemies = this.game.add.group();
            for (let i = 0; i < 30; i++) {
                var bat = new Shapeshifter.Bat(this.game, this.game.rnd.between(40, Shapeshifter.Game.WORLD_WIDTH - 40), -200, this.enemyBulletPool, this.player);
                this.enemies.add(bat);
                bat.exists = false;
                bat.alive = false;
            }
            this.game.camera.follow(this.player, Phaser.Camera.FOLLOW_PLATFORMER);
            this.game.camera.x = 50;
            this.game.camera.y = 0;
            if (!Shapeshifter.Game.EMPTY_ROOM) {
                var wave1Timer = this.game.time.events.add(Phaser.Timer.SECOND, () => this.startBatWave("BrownBat", 300, 30), this);
                var powerUp1Timer = this.game.time.events.add(Phaser.Timer.SECOND * 7, () => {
                    var powerUp1 = new Shapeshifter.PowerUp(this.game, Shapeshifter.PowerUpType.Wizard);
                    this.powerUps.add(powerUp1);
                }, this);
                var wave2Timer = this.game.time.events.add(Phaser.Timer.SECOND * 9, () => this.startBatWave("BlueBat", 300, 20), this);
                var wave3Timer = this.game.time.events.add(Phaser.Timer.SECOND * 18, () => this.startBatWave("OrangeBat", 600, 10), this);
                var wave4Timer = this.game.time.events.add(Phaser.Timer.SECOND * 28, () => this.startBatWave("RedBat", 800, 6), this);
                var victoryCondition = this.game.time.events.add(Phaser.Timer.SECOND * 40, this.stageDefeated, this);
            }
        }
        update() {
            this.physics.arcade.overlap(this.player, this.enemies, this.playerVsEnemy, null, this);
            this.physics.arcade.overlap(this.player.playerWeapon.bullets, this.enemies, this.playerBulletVsEnemy, null, this);
            this.physics.arcade.overlap(this.player, this.powerUps, this.playerVsPowerUp, null, this);
            this.physics.arcade.overlap(this.player, this.enemyBulletPool, this.playerVsEnemyBullet, null, this);
            this.background.tilePosition.y += Shapeshifter.Game.GAME_SCROLL_SPEED;
            this.debrisLayer.y += Shapeshifter.Game.GAME_SCROLL_SPEED;
            this.debrisLayer.x = this.game.camera.x;
        }
        playerVsEnemy(player, enemy) {
            if (!enemy.alive)
                return;
            enemy.kill();
            if (player.takeDamageCooldown < 1) {
                player.takeDamage(20);
                this.playerHurtSound.play();
            }
        }
        playerBulletVsEnemy(bullet, enemy) {
            enemy.kill();
            bullet.kill();
            this.enemyDyingSound.play();
        }
        playerVsPowerUp(player, powerUp) {
            powerUp.kill();
            this.getPowerUpSound.play();
            if (powerUp.powerUpType == Shapeshifter.PowerUpType.Wizard) {
                player.hasWizardForm = true;
            }
            else {
                player.hasCrowForm = true;
            }
        }
        playerVsEnemyBullet(player, bullet) {
            bullet.kill();
            if (player.takeDamageCooldown < 1) {
                player.takeDamage(40);
                this.playerHurtSound.play();
            }
        }
        startBatWave(batTypeName, delay, repeatCount) {
            this.game.time.events.repeat(delay, repeatCount, () => {
                let bat = this.enemies.getFirstExists(false);
                if (bat)
                    bat.reviveBat(batTypeName);
            });
        }
        createEnemyBulletPool() {
            this.enemyBulletPool = this.game.add.group();
            this.enemyBulletPool.enableBody = true;
            this.enemyBulletPool.physicsBodyType = Phaser.Physics.ARCADE;
            this.enemyBulletPool.createMultiple(100, 'batBullet');
            this.enemyBulletPool.setAll('anchor.x', 0.5);
            this.enemyBulletPool.setAll('anchor.y', 0.5);
            this.enemyBulletPool.setAll('outOfBoundsKill', true);
            this.enemyBulletPool.setAll('checkWorldBounds', true);
        }
        stageDefeated() {
            if (this.player.playerState != Shapeshifter.PlayerState.Dead) {
                let textStyle = { font: "20px Arial", fill: "#ff0000", align: "center" };
                let gameOverText = `YOU ARE WINNER
        Sorry there is so little "game" here, thanks for playing anyway`;
                let text = this.game.add.text(0, 0, gameOverText, textStyle);
                text.fixedToCamera = true;
                text.cameraOffset.setTo(-20, 300);
            }
        }
        render() {
            if (Shapeshifter.Game.DEBUG_MODE) {
                this.game.debug.text(`takeDamageCooldown: ${this.player.takeDamageCooldown}
        hasWizardForm: ${this.player.hasWizardForm}`, 10, 120);
                this.game.debug.text(`Player X Scale: ${this.player.scale.x} Debris Layer Y: ${this.debrisLayer.y}`, 10, 140);
                this.game.debug.cameraInfo(this.game.camera, 32, 32);
            }
        }
    }
    Shapeshifter.Level1 = Level1;
})(Shapeshifter || (Shapeshifter = {}));
var Shapeshifter;
(function (Shapeshifter) {
    class Game extends Phaser.Game {
        constructor() {
            super(Shapeshifter.Game.GAME_WIDTH, Shapeshifter.Game.GAME_HEIGHT, Phaser.AUTO, 'content', null);
            this.state.add('Boot', Shapeshifter.Boot, false);
            this.state.add('Preloader', Shapeshifter.Preloader, false);
            this.state.add('MainMenu', Shapeshifter.MainMenu, false);
            this.state.add('Level1', Shapeshifter.Level1, false);
            this.state.start('Boot');
        }
        static get DEBUG_MODE() { return false; }
        static get MUTE_SOUND() { return false; }
        static get EMPTY_ROOM() { return false; }
        static get GAME_WIDTH() { return 600; }
        static get GAME_HEIGHT() { return 800; }
        static get WORLD_WIDTH() { return 700; }
        static get WORLD_HEIGHT() { return 800; }
        static get GAME_SCROLL_SPEED() { return 1; }
        static get VELOCITY_TO_MATCH_SCROLL_SPEED() { return 60; }
        static get RABBIT_WALK_SPEED() { return 250; }
    }
    Shapeshifter.Game = Game;
})(Shapeshifter || (Shapeshifter = {}));
window.onload = () => {
    game = new Shapeshifter.Game();
};
//# sourceMappingURL=game.js.map