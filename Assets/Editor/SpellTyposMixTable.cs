using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Every mix the book starts with, built from the answers given for the 253 combinations.
// Rule tokens (all optional) are read by Apply and written into the SpellMix rule fields:
//   every / last / tick / reflect  = when the rider lands (every hit, last hit, each poison tick, each mirror bounce)
//   enemy / self / all             = who the rider affects (default: the enemy, or the caster for self-only spells)
//   bydamage   the rider's amount is the damage the body did      stack      statuses build up with every hit
//   permanent  the status never wears off                          first      rider lands before the body
//   onlyhit    rider only if the body did something               visual     rider is only a visual
//   bodyvisual body is only a visual                               again      cast twice with AGAIN! between
//   area       body does no damage, rider reaches the arena        pierce     rider destroys the enemy shield first
//   keepscars  salt/poop do not close scars                        nodmg      the caster takes no self damage
//   arena      the next spell of the rider's element is x2         shades     rainbow uses shades of one colour
//   blood      a blood pact (the pact perk column says what it grants)
internal static class SpellTyposMixTable
{
    internal static readonly (string Body, string Rider, string Name, string Flags, string Pact, string Note)[] Table =
    {
        ("blot", "cleanse", "Purifying Fury", "", "None", ""),
        ("blot", "fear", "Dread Fury", "", "None", ""),
        ("blot", "fur", "Furry Fury", "", "None", ""),
        ("blot", "heal", "Healing Fury", "", "None", ""),
        ("blot", "mirror", "Mirrored Fury", "again", "None", "vai duas ondas de blot, antes de ir a segunda onda de blot aparece na tela \"AGAIN!\""),
        ("blot", "sacrifice", "Blood Fury", "blood", "DoubleAll", "in this sacrifice blot is sacrificed and a message appears \"weakling sacrificed\" 2x damage para todos os spells até o final do jogo"),
        ("blot", "shield", "Shielded Fury", "", "None", ""),
        ("blot", "water", "Soaked Fury", "", "None", ""),
        ("bolt", "blot", "Furious Bolt", "", "None", ""),
        ("bolt", "cleanse", "Purifying Bolt", "", "None", ""),
        ("bolt", "fear", "Dread Bolt", "", "None", "tudo como descrito mas  o raio deve ser da cor do rider"),
        ("bolt", "fur", "Furry Bolt", "pierce visual", "None", "raio com fur que ignora shield o raio deve ser da cor do rider"),
        ("bolt", "heal", "Healing Bolt", "enemy", "None", "raio que cura o inimigo o raio deve ser da cor do rider"),
        ("bolt", "ice", "Frost Bolt", "", "None", "só congela depois do inimigo ser atingido pelo raio, o raio deve ser da cor do rider"),
        ("bolt", "mirror", "Mirrored Bolt", "again", "None", "vai duas ondas de raio, antes de ir a segunda onda de bola de fogo aparece na tela \"AGAIN!\" o raio deve ser da cor do rider"),
        ("bolt", "poison", "Venom Bolt", "", "None", ""),
        ("bolt", "poop", "Stinky Bolt", "", "None", ""),
        ("bolt", "rainbow", "Prism Bolt", "", "None", "tudo igual ao descrito mas o raio deve ser da cor do rider"),
        ("bolt", "sacrifice", "Blood Bolt", "blood", "Boost", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento o bolt e qualquer variação dele fica enorme e dá o dobro do dano"),
        ("bolt", "salt", "Salted Bolt", "", "None", ""),
        ("bolt", "shatter", "Shattering Bolt", "", "None", "só aplica o efeito shatter depois do raio atingir"),
        ("bolt", "shield", "Shielded Bolt", "", "None", ""),
        ("water", "bolt", "Storm Bolt", "", "None", "aceito mas inverte"),
        ("cleanse", "sacrifice", "Blood Cleanse", "blood", "StatusImmune", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento o player é imune a status"),
        ("cut", "blot", "Furious Cut", "", "None", ""),
        ("cut", "bolt", "Storm Cut", "last enemy", "None", "no ultimo cut cai um lightening no inimigo"),
        ("cut", "cleanse", "Purifying Cut", "every", "None", "limpa um status por cada cut se houver mais status do que cut não limpa tudo"),
        ("cut", "fear", "Dread Cut", "", "None", ""),
        ("cut", "fire", "Flaming Cut", "", "None", ""),
        ("cut", "fur", "Furry Cut", "pierce visual", "None", "o cut vem com uma bola de pelo que faz com que os cuts ignorem shield"),
        ("cut", "gamble", "Lucky Cut", "", "None", "espera o gamble terminar, dá muitos cuts mas no caso de jackpot para além dos muitos cuts dá um cut maior e mais epico"),
        ("cut", "heal", "Healing Cut", "", "None", ""),
        ("cut", "ice", "Frost Cut", "", "None", ""),
        ("cut", "meteor", "Meteoric Cut", "enemy nodmg", "None", "deixa cair meteoros mas o player corta os meteoros que cairiam nele mesmo evitando dano"),
        ("cut", "mirror", "Mirrored Cut", "", "None", ""),
        ("cut", "poison", "Venom Cut", "", "None", ""),
        ("cut", "poop", "Stinky Cut", "last keepscars", "None", "o poop neste caso serve só para dar dano incremental, não destroi as cicatrizes"),
        ("cut", "rainbow", "Prism Cut", "", "None", ""),
        ("cut", "sacrifice", "Blood Cut", "blood", "None", "neste caso o sacrifice é a vida retirada ao inimigo com o cut"),
        ("cut", "salt", "Salted Cut", "last keepscars", "None", "o sal neste caso serve só para dar dano incremental, não destroi as cicatrizes"),
        ("cut", "shatter", "Shattering Cut", "last", "None", "o ultimo corte estilhaça o alvo com o centro do efeito sendo o uktimo estilhaço"),
        ("cut", "shield", "Shielded Cut", "every", "None", "dá um shield por cada cut"),
        ("cut", "water", "Soaked Cut", "every", "None", "dá um wet por cada cut"),
        ("explosion", "blot", "Furious Blast", "", "None", ""),
        ("explosion", "bolt", "Storm Blast", "", "None", "A explosão espera um raio enorme cair no mapa antes de explodir no sitio onde o raio caiu, o resto da lógica da explosão metem-se igual"),
        ("explosion", "cleanse", "Purifying Blast", "area all", "None", "Explosão  que em vez de dar dano dá clense a todos na arena"),
        ("explosion", "cut", "Razor Blast", "area all", "None", "explosão de cortes que dá o maximo de cortes a todos na arena"),
        ("explosion", "fear", "Dread Blast", "area enemy permanent", "None", "uma explosão de fear que em vez de matar mete fear permanente no inimigo imune ao player"),
        ("explosion", "fire", "Flaming Blast", "arena", "None", "explosão de fogo deixa fogo na arena proximo ataque de fogo será mais forte"),
        ("explosion", "fur", "Furry Blast", "nodmg", "None", "devido ao fur  o player sobrevive"),
        ("explosion", "gamble", "Lucky Blast", "", "None", "explosão que mata todos após resultado do gamble mas se calhar jackpot o player não leva dano"),
        ("explosion", "git", "Binary Blast", "", "None", "uma explosão de bits explode uma x quantidade de vezes só a ultima explosão realmente finaliza todos"),
        ("explosion", "heal", "Healing Blast", "area all", "None", "explosão que em vez de dar dano cura todos na arena"),
        ("explosion", "ice", "Frost Blast", "arena", "None", "explosão de fogo deixa fogo na arena proximo ataque de gelo será mais forte"),
        ("explosion", "meteor", "Meteoric Blast", "", "None", "A explosão espera um meteoro enorme cair no mapa antes de explodir no sitio onde o meteoro caiu, o resto da lógica da explosão metem-se igual"),
        ("explosion", "mirror", "Mirrored Blast", "", "None", "duas explosões, a primeira devido ao feitiço a segunda por ser refletida pelo mirror"),
        ("explosion", "poison", "Venom Blast", "area all", "None", "explosão que em vez de dar dano dá poison a todos no mapa"),
        ("explosion", "poop", "Stinky Blast", "area all", "None", "explosão de poop que em vez de dar dano aplica poop a todos no  mapa"),
        ("explosion", "rainbow", "Prism Blast", "area all", "None", "Explosão arco iris que em vez de dar dano dá os status maus a todos na arena"),
        ("explosion", "sacrifice", "Blood Blast", "blood", "DoubleAll", "quando o sacrifice é feito desta maneira o 2x damage buff é permanente para o resto da run"),
        ("explosion", "salt", "Salted Blast", "area all", "None", "explosão de sal que em vez de matar aplica salt a todos na arena"),
        ("explosion", "shatter", "Shattering Blast", "visual", "None", "explosão que dá shatter na propria arena, puramente visual o resto da lógica da explosão mantêm se"),
        ("explosion", "shield", "Shielded Blast", "area all", "None", "explosão que em vez de dar dano dá shield a todos"),
        ("explosion", "water", "Soaked Blast", "area all", "None", "explosão de agua que em vez de dar dano deixa todos no mapa wet"),
        ("familiar", "blot", "Fury Pet", "", "None", ""),
        ("familiar", "bolt", "Storm Pet", "", "None", ""),
        ("familiar", "cleanse", "Holy Pet", "", "None", ""),
        ("familiar", "cut", "Blade Pet", "", "None", ""),
        ("familiar", "explosion", "Bomb Pet", "", "None", "familiar explosion mata apenas o familiar e o inimigo e gasta o spell explosion na mesma"),
        ("familiar", "fear", "Scare Pet", "", "None", ""),
        ("familiar", "fire", "Fire Pet", "", "None", ""),
        ("familiar", "fur", "Fluffy Pet", "", "None", ""),
        ("familiar", "gamble", "Lucky Pet", "", "None", "Todos os turnos ele roda minislots que ao acertar jackpot dão metade do dano normal de um jackpot ao inimigo"),
        ("familiar", "git", "Binary Pet", "", "None", ""),
        ("familiar", "heal", "Healing Pet", "", "None", ""),
        ("familiar", "ice", "Frost Pet", "", "None", ""),
        ("familiar", "meteor", "Comet Pet", "", "None", "os meteoros afetam primeiro o pet e só depois a mim"),
        ("familiar", "mirror", "Mirror Pet", "", "None", "sempre que devolver um feitiço perde 2 de vida"),
        ("familiar", "poison", "Venom Pet", "", "None", ""),
        ("familiar", "poop", "Stinky Pet", "", "None", ""),
        ("familiar", "rainbow", "Rainbow Pet", "", "None", ""),
        ("familiar", "sacrifice", "Blood Pet", "blood", "StrongFamiliar", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento os familiares ficam com o dobro da vida e do dano e ficam enormes"),
        ("familiar", "salt", "Salty Pet", "", "None", ""),
        ("familiar", "shatter", "Crystal Pet", "", "None", ""),
        ("familiar", "shield", "Guardian Pet", "", "None", ""),
        ("familiar", "water", "Water Pet", "", "None", ""),
        ("fear", "cleanse", "Purifying Fear", "", "None", ""),
        ("fear", "fur", "Furry Fear", "", "None", ""),
        ("fear", "heal", "Healing Fear", "", "None", ""),
        ("fear", "mirror", "Mirrored Fear", "again", "None", "vai duas ondas de fear, antes de ir a segunda onda de fear aparece na tela \"AGAIN!\""),
        ("fear", "sacrifice", "Blood Fear", "blood", "FearKills", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento se o inimigo tiver 10 ou mais fear stacks ele mata-se"),
        ("fear", "shield", "Shielded Fear", "", "None", ""),
        ("fire", "blot", "Furious Fireball", "", "None", ""),
        ("fire", "bolt", "Storm Fireball", "", "None", "o raio só cai depois da bola de fogo acertar o alvo"),
        ("fire", "cleanse", "Purifying Fireball", "", "None", ""),
        ("fire", "fear", "Dread Fireball", "", "None", "a bola de fogo fica roxa para além do efeito que mencionaste"),
        ("fire", "fur", "Furry Fireball", "pierce visual", "None", "uma bola de fogo com fur que destroi shield e dá dano"),
        ("fire", "heal", "Healing Fireball", "", "None", "para além disso a bola de fogo deve ser fogo verde"),
        ("fire", "ice", "Frost Fireball", "", "None", ""),
        ("fire", "mirror", "Mirrored Fireball", "again", "None", "vai duas ondas de bola de fogo, antes de ir a segunda onda de bola de fogo aparece na tela \"AGAIN!\""),
        ("fire", "poison", "Venom Fireball", "", "None", ""),
        ("fire", "poop", "Stinky Fireball", "", "None", ""),
        ("fire", "rainbow", "Prism Fireball", "", "None", ""),
        ("fire", "sacrifice", "Blood Fireball", "blood", "Boost", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento a bola de fogo e qualquer variação dele fica enorme e dá o dobro do dano"),
        ("fire", "salt", "Salted Fireball", "", "None", ""),
        ("fire", "shatter", "Shattering Fireball", "first", "None", "dá shatter primeiro depois burn, o efeito do shatter só é disparado quando a bola de fogo toca no inimigo"),
        ("fire", "shield", "Shielded Fireball", "", "None", ""),
        ("fire", "water", "Soaked Fireball", "", "None", "o efeito visual deveria ser uma bola de fogo e de agua a serem lançadas em helice entre si"),
        ("fur", "cleanse", "Purifying Fur", "", "None", ""),
        ("fur", "heal", "Healing Fur", "", "None", ""),
        ("fur", "sacrifice", "Blood Fur", "blood", "PierceShield", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento todas as habilidades do jogador ignoram shield do inimigo e vão com o efeito visual do fur"),
        ("fur", "shield", "Shielded Fur", "", "None", ""),
        ("gamble", "blot", "Furious Gamble", "", "None", ""),
        ("gamble", "bolt", "Storm Gamble", "", "None", ""),
        ("gamble", "cleanse", "Purifying Gamble", "onlyhit", "None", "Cada simbolo igual dá cleanse a um status, caso jackpot torna o player imune a status"),
        ("gamble", "fear", "Dread Gamble", "onlyhit permanent", "None", "dá fear permanente caso jackpot, caso contrário nada acontece, os simbulos são coisas assustadoras como caveiras morcegos etc"),
        ("gamble", "fire", "Flaming Gamble", "onlyhit", "None", "cada slot igual lança uma bola de fogo, caso jackpot lança uma bola de fogo muito grande, após jackpot o simbolo do jackpot vira fogo, o mapa fica com varios fogos"),
        ("gamble", "fur", "Furry Gamble", "onlyhit visual", "None", "caso jackpot dá te bola de pelo por 3 turnos e deixa o mapa cheio de bolas de pelo, caso contrário nada acontece, quando acontece o jackpot o simbolo que foi jackpoted vira fur antes da animação acabar"),
        ("gamble", "heal", "Healing Gamble", "onlyhit", "None", "caso jackpot cura te a vida toda e dá te o dobro da vida maxima, caso contrário cura um pouco a cada simbolo igual, os simbulos são cruzes médicas de diferentes cores"),
        ("gamble", "ice", "Frost Gamble", "onlyhit permanent", "None", "caso jackpot o mapa fica cheio de cubos e espinhos de gelo e o inimigo fica congelado premanetemente, caso contrario nada acontece"),
        ("gamble", "meteor", "Meteoric Gamble", "onlyhit nodmg", "None", "caso jackpot caem meteoros e o player não leva dano, caso contrário nada acontece, caso jackpot os simbolos do jackpot viram meteoros antes da animação acabar"),
        ("gamble", "mirror", "Mirrored Gamble", "", "None", "cada simbolo igual dá um mirror ao player, caso aconteça jackpot o inimigo começa a fazer gamble, se o inimigo não conseguir jackpot leva dano se conseguir jackpot não leva dano e o jogador perde os mirrors"),
        ("gamble", "poison", "Venom Gamble", "onlyhit", "None", "a cada simbolo igual adiciona um poison por cada  e em caso de jackpot o inimigo ganha 10 poison"),
        ("gamble", "poop", "Stinky Gamble", "onlyhit", "None", "a cada simbolo igual adiciona um veneno por cada duas cicatrizes sem gastar as cicatrizes e em caso de jackpot um poop enorme cai em cima do inimigo e o inimigo recebe 10 veneno por cada cicatriz"),
        ("gamble", "rainbow", "Prism Gamble", "onlyhit", "None", "caso não dê jackpot nada acontece, caso dê jackpot o efeito rainbow acontece o o cenario fica rainbow  também para além disso o player ganha todos os status positivos, os simbolos são cor de arco iris"),
        ("gamble", "sacrifice", "Blood Gamble", "blood", "SureJackpot", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento gamble e todas as suas variações têm 100% de chance de dar jackpot"),
        ("gamble", "salt", "Salted Gamble", "onlyhit", "None", "caso jackpot cria pilhas de salt no mapa agora o sal é aplicado automaticamente sempre que o inimigo tiver cuts suficientes"),
        ("gamble", "shatter", "Shattering Gamble", "onlyhit", "None", "caso jackpot tem o efeito do explosion shatter mas o player não leva dano, caso contrário nada acontece"),
        ("gamble", "shield", "Shielded Gamble", "onlyhit", "None", "cada simbolo igual dá shield ao player e em caso de jackpot o player absorve 90% da vida do inimigo e transforma em shield"),
        ("gamble", "water", "Soaked Gamble", "onlyhit", "None", "cada simbolo igual adiciona wet e em caso de jackpot um tsunami engole o inimigo dá bastante dano e bastantes stacks de wet"),
        ("git", "blot", "Furious Git", "", "None", ""),
        ("git", "bolt", "Storm Git", "", "None", ""),
        ("git", "cleanse", "Purifying Git", "every", "None", "cada hit limpa um status se tiver mais que o numero de hits do git não limpa todos"),
        ("git", "cut", "Razor Git", "every", "None", "corta uma ver por cada hit"),
        ("git", "fear", "Dread Git", "every stack", "None", "cada hit adiciona mais fear"),
        ("git", "fire", "Hellfire Git", "every stack", "None", "cada hit adiciona mais burn"),
        ("git", "fur", "Furry Git", "pierce visual", "None", "os numeros são disparados com bolas de pelo á sua volta, destroi shields"),
        ("git", "gamble", "Lucky Git", "", "None", "dá gamble só que os simbolos são só 0 e 1 então fica mais facil ter jackpot mas o dano é menor"),
        ("git", "heal", "Healing Git", "every enemy", "None", "cada bit cura o inimigo"),
        ("git", "ice", "Frost Git", "every stack", "None", "cada hit congela mais"),
        ("git", "meteor", "Meteoric Git", "", "None", "Caem meteoros mas o trail deles é os bits"),
        ("git", "mirror", "Mirrored Git", "", "None", ""),
        ("git", "poison", "Toxic Git", "every", "None", "cada hit adiciona mais poison"),
        ("git", "poop", "Stinky Git", "", "None", ""),
        ("git", "rainbow", "Rainbow Git", "", "None", ""),
        ("git", "sacrifice", "Blood Git", "blood", "LongGit", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento git e qualquer variação passa a ser 2 vezes mais longo com 2 vezes mais hits e o numero passa a ser 171004042026"),
        ("git", "salt", "Salted Git", "every", "None", "salga uma ver por cada hit"),
        ("git", "shatter", "Shattering Git", "every", "None", "estilhaça uma ver por cada hit"),
        ("git", "shield", "Shielded Git", "every", "None", "dá te escudo por cada hit do git"),
        ("git", "water", "Soaked Git", "every", "None", "dá 1 wet por cada hit do git"),
        ("heal", "cleanse", "Purifying Heal", "", "None", ""),
        ("heal", "sacrifice", "Blood Heal", "blood", "FullHeal", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento o spell heal e todas as suas variações curam a vida toda"),
        ("ice", "blot", "Furious Ice", "", "None", ""),
        ("ice", "cleanse", "Purifying Ice", "", "None", ""),
        ("ice", "fear", "Dread Ice", "", "None", ""),
        ("ice", "fur", "Furry Ice", "", "None", ""),
        ("ice", "heal", "Healing Ice", "", "None", ""),
        ("ice", "mirror", "Mirrored Ice", "again", "None", "vai duas ondas de ice, antes de ir a segunda onda de ice aparece na tela \"AGAIN!\""),
        ("ice", "poop", "Stinky Ice", "", "None", ""),
        ("ice", "sacrifice", "Blood Ice", "blood", "IceHeal", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento sempre que o jogador mandar ice ou qualquer variação de ice para além dos efeitos dessa variação serem aplicados o jogador congela-se num gelo de sangue e cura-se, no final do turno o gelo descongela sem fazer o player perder turnos"),
        ("ice", "salt", "Salted Ice", "", "None", ""),
        ("ice", "shatter", "Deep Freeze", "", "None", ""),
        ("ice", "shield", "Shielded Ice", "", "None", ""),
        ("ice", "water", "Soaked Ice", "", "None", ""),
        ("meteor", "blot", "Furious Meteor", "", "None", ""),
        ("meteor", "bolt", "Storm Meteor", "every all", "None", "o efeito to choque é aplicado a todos, e cai raios onde o meteoro cair"),
        ("meteor", "cleanse", "Purifying Meteor", "every all", "None", "os hits dos meteoros dão clense tanto o player quanto o inimigo, os meteoros são cruzes médicas e têm trail de cruzes"),
        ("meteor", "fear", "Dread Meteor", "", "None", ""),
        ("meteor", "fire", "Flaming Meteor", "", "None", ""),
        ("meteor", "fur", "Furry Meteor", "pierce visual", "None", "faz meteoros com fur cairem, destroi shields e todos"),
        ("meteor", "heal", "Healing Meteor", "every all", "None", "os hits dos meteoros curam tanto o player quanto o inimigo os meteoros são cruzes médicas e têm trail de cruzes"),
        ("meteor", "ice", "Frost Meteor", "", "None", ""),
        ("meteor", "mirror", "Mirrored Meteor", "again", "None", "cai duas ondas de meteoros, antes de cair a segunda onde de meteoros aparece na tela \"AGAIN!\""),
        ("meteor", "poison", "Venom Meteor", "all", "None", "envenena todos"),
        ("meteor", "poop", "Stinky Meteor", "all", "None", "a todos"),
        ("meteor", "rainbow", "Prism Meteor", "", "None", "com um trail de arco iris"),
        ("meteor", "sacrifice", "Blood Meteor", "blood", "SpareSelf", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento meteor e qualquer variação dele não afeta mais negativamente o player"),
        ("meteor", "salt", "Salted Meteor", "every all", "None", "os hits dos meteoros aplicam salt tanto nos inimigos quanto no player, os meteoros são feitos de salt e têm um trail de salt"),
        ("meteor", "shatter", "Shattering Meteor", "every all", "None", "visualmente o meteoro dá shatter onde cai, cada hit do meteoro dá shatter tanto no player quanto no inimigo"),
        ("meteor", "shield", "Shielded Meteor", "every all", "None", "cada hit do meteoro dá shield a todos"),
        ("meteor", "water", "Soaked Meteor", "all", "None", "molha todos"),
        ("mirror", "cleanse", "Purifying Mirror", "reflect bydamage", "None", "dá te cleanse pelo dano da habilidade refletida, se houver status a mais não cura tudo"),
        ("mirror", "fur", "Furry Mirror", "", "None", ""),
        ("mirror", "heal", "Healing Mirror", "reflect bydamage", "None", "cura te pelo dano da habilidade refletida"),
        ("mirror", "sacrifice", "Blood Mirror", "blood", "CastTwice", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento todas as habilidades do jogador são lançadas duas vezes com uma mensagem na tela a dizer \"Blood mirror: AGAIN!\""),
        ("mirror", "shield", "Shielded Mirror", "reflect bydamage", "None", "dá te escudo pelo dano da habilidade refletida"),
        ("poison", "blot", "Furious Poison", "", "None", ""),
        ("poison", "cleanse", "Purifying Poison", "tick", "None", "esta habilidade dá cleanse sempre que o poison no inimigo der tic"),
        ("poison", "fear", "Dread Poison", "", "None", ""),
        ("poison", "fur", "Furry Poison", "", "None", ""),
        ("poison", "heal", "Healing Poison", "tick bydamage", "None", "esta habilidade cura o player pela quantidade de dano que o poison der ao inimigo e é curado por essa quantidade sempre que o poison do inimigo der tic"),
        ("poison", "ice", "Frost Poison", "", "None", ""),
        ("poison", "mirror", "Mirrored Poison", "", "None", ""),
        ("poison", "poop", "Stinky Poison", "", "None", ""),
        ("rainbow", "poison", "Prism Poison", "bodyvisual permanent shades", "None", "dá poison permanente e as cores do arco iris são variações de tonalidade da cor do poison"),
        ("poison", "sacrifice", "Blood Poison", "blood", "SelfPoison", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento qualquer ação que o inimigo faça aplica poison nele mesmo"),
        ("poison", "salt", "Salted Poison", "", "None", ""),
        ("poison", "shatter", "Shattering Poison", "tick", "None", "estilhaça o alvo sempre que o poison no inimigo der tic"),
        ("poison", "shield", "Shielded Poison", "tick", "None", "dá escudo sempre que poison no inimigo der tic"),
        ("poison", "water", "Soaked Poison", "", "None", ""),
        ("poop", "blot", "Furious Poop", "", "None", ""),
        ("poop", "cleanse", "Purifying Poop", "onlyhit", "None", "dá cleanse apenas se o veneno for aplicado pela quantidade de vezes que veneno foi aplicado"),
        ("poop", "fear", "Dread Poop", "onlyhit", "None", "assusta o inimigo só quando o veneno é aplicado"),
        ("poop", "fur", "Furry Poop", "onlyhit visual", "None", "só aplica fur se o veneno for aplicado"),
        ("poop", "heal", "Healing Poop", "onlyhit", "None", "cura apenas se o veneno for aplicado"),
        ("poop", "mirror", "Mirrored Poop", "again", "None", "vai duas ondas de poop, antes de ir a segunda onda de poop aparece na tela \"AGAIN!\""),
        ("poop", "sacrifice", "Blood Poop", "blood", "KeepScars", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento poop e as suas variações são enormes não gastaram os cuts e criaram mais cuts"),
        ("poop", "shield", "Shielded Poop", "onlyhit", "None", "apenas dá escudo se o veneno for aplicado"),
        ("poop", "water", "Soaked Poop", "onlyhit", "None", "só molha se o veneno for aplicado"),
        ("rainbow", "blot", "Furious Rainbow", "", "None", ""),
        ("rainbow", "cleanse", "Purifying Rainbow", "", "None", ""),
        ("rainbow", "fear", "Dread Rainbow", "bodyvisual permanent shades", "None", "dá fear permanente e as cores do arco iris são variações de tonalidade da cor do fear"),
        ("rainbow", "fur", "Furry Rainbow", "", "None", ""),
        ("rainbow", "heal", "Healing Rainbow", "shades", "None", "aceito mas as cores do arco iris são variações de tonalidade da cor do heal"),
        ("rainbow", "ice", "Frost Rainbow", "bodyvisual permanent shades", "None", "dá ice permanente e as cores do arco iris são variações de tonalidade da cor do ice"),
        ("rainbow", "mirror", "Mirrored Rainbow", "again", "None", "vai duas ondas de rainbow, antes de ir a segunda onda de rainbow aparece na tela \"AGAIN!\""),
        ("rainbow", "poop", "Stinky Rainbow", "", "None", ""),
        ("rainbow", "sacrifice", "Blood Rainbow", "blood", "RainbowStart", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento sempre que o inimigo for iniciar o seu turno os efeitos de rainbow são aplicados e a tela fica arco iris por 2 segundos"),
        ("rainbow", "salt", "Salted Rainbow", "", "None", ""),
        ("rainbow", "shield", "Shielded Rainbow", "", "None", ""),
        ("rainbow", "water", "Soaked Rainbow", "bodyvisual permanent shades", "None", "dá wet permanente e as cores do arco iris são variações de tonalidade da cor do water"),
        ("salt", "blot", "Furious Salt", "", "None", ""),
        ("salt", "cleanse", "Purifying Salt", "bydamage", "None", "dá clense pelo valor de dano dado, se o valor for menor que o numero de status aplicados no player não cura todos"),
        ("salt", "fear", "Dread Salt", "", "None", ""),
        ("salt", "fur", "Furry Salt", "", "None", ""),
        ("salt", "heal", "Healing Salt", "bydamage", "None", "cura te pelo valor de dano dado"),
        ("salt", "mirror", "Mirrored Salt", "again", "None", "vai duas ondas de salt, antes de ir a segunda onda de salt aparece na tela \"AGAIN!\""),
        ("salt", "poop", "Stinky Salt", "", "None", ""),
        ("salt", "sacrifice", "Blood Salt", "blood", "KeepScars", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento salt e as suas variações não gastaram os cuts e criaram mais cuts"),
        ("salt", "shield", "Shielded Salt", "", "None", ""),
        ("salt", "water", "Soaked Salt", "bydamage", "None", "molha pelo valor de dano dado"),
        ("shatter", "blot", "Furious Shatter", "", "None", ""),
        ("shatter", "cleanse", "Purifying Shatter", "", "None", ""),
        ("shatter", "fear", "Dread Shatter", "", "None", ""),
        ("shatter", "fur", "Furry Shatter", "pierce visual", "None", "shatter com fur que destroi shields e dá mais dano se o inimigo tiver shields"),
        ("shatter", "heal", "Healing Shatter", "", "None", ""),
        ("shatter", "mirror", "Mirrored Shatter", "again", "None", "vai duas ondas de Shatter, antes de ir a segunda onda de Shatter aparece na tela \"AGAIN!\""),
        ("shatter", "poop", "Stinky Shatter", "", "None", ""),
        ("shatter", "rainbow", "Prism Shatter", "", "None", ""),
        ("shatter", "sacrifice", "Blood Shatter", "blood", "AutoShatter", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento sempre que o inimigo estiver congelado o shatter acontece automaticamente sem desperdiçar o turno do player"),
        ("shatter", "salt", "Salted Shatter", "", "None", ""),
        ("shatter", "shield", "Shielded Shatter", "", "None", ""),
        ("shatter", "water", "Soaked Shatter", "", "None", ""),
        ("shield", "cleanse", "Purifying Shield", "", "None", ""),
        ("shield", "heal", "Healing Shield", "", "None", ""),
        ("shield", "sacrifice", "Blood Shield", "blood", "ShieldEachTurn", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento todos os turnos o player ganha shields"),
        ("water", "cleanse", "Purifying Splash", "", "None", ""),
        ("water", "fear", "Dread Splash", "", "None", ""),
        ("water", "fur", "Furry Splash", "", "None", ""),
        ("water", "heal", "Healing Splash", "", "None", ""),
        ("water", "mirror", "Mirrored Splash", "again", "None", "vai duas ondas de water, antes de ir a segunda onda de water aparece na tela \"AGAIN!\""),
        ("water", "sacrifice", "Blood Splash", "blood", "WaterKills", "para este sacrificio o player só pode escolher sacrificar metade da sua vida maxima para o resto do jogo, o seu familiar ou então dois feitiços aleatórios escolhidos pelo jogo, a partir desse momento se o inimigo tiver 10 ou mais wet stacks ele afoga-se e morre instantaneamente"),
        ("water", "shield", "Shielded Splash", "", "None", ""),
    };

    private static readonly (string Word, Color Color)[] Colors =
    {
        ("blot", new Color(0.85f, 0.2f, 0.2f)),
        ("bolt", new Color(1f, 0.95f, 0.3f)),
        ("cleanse", new Color(0.75f, 1f, 1f)),
        ("cut", new Color(0.9f, 0.25f, 0.3f)),
        ("explosion", new Color(1f, 0.55f, 0.1f)),
        ("familiar", new Color(1f, 0.9f, 0.65f)),
        ("fear", new Color(0.6f, 0.25f, 0.85f)),
        ("fire", new Color(1f, 0.5f, 0.1f)),
        ("fur", new Color(0.72f, 0.5f, 0.3f)),
        ("gamble", new Color(1f, 0.8f, 0.2f)),
        ("git", new Color(0.2f, 1f, 0.45f)),
        ("heal", new Color(0.35f, 1f, 0.5f)),
        ("ice", new Color(0.55f, 0.9f, 1f)),
        ("meteor", new Color(1f, 0.38f, 0.12f)),
        ("mirror", new Color(0.8f, 0.85f, 0.95f)),
        ("poison", new Color(0.55f, 0.9f, 0.2f)),
        ("poop", new Color(0.45f, 0.3f, 0.15f)),
        ("rainbow", Color.white),
        ("sacrifice", new Color(0.65f, 0.05f, 0.1f)),
        ("salt", new Color(0.92f, 0.92f, 0.98f)),
        ("shatter", new Color(0.7f, 0.95f, 1f)),
        ("shield", new Color(0.6f, 0.72f, 0.95f)),
        ("water", new Color(0.3f, 0.6f, 1f))
    };

    private static Object EffectOf(SerializedProperty entries, string word)
    {
        for (int i = 0; i < entries.arraySize; i++)
        {
            SerializedProperty e = entries.GetArrayElementAtIndex(i);
            if (CastResolver.Normalize(e.FindPropertyRelative("_word").stringValue) == word)
                return e.FindPropertyRelative("_effect").objectReferenceValue;
        }
        return null;
    }

    // Gives every effect its signature colour (the colour a mix takes when the spell is the rider).
    internal static void ApplyColors(SpellBook book)
    {
        var so = new SerializedObject(book);
        SerializedProperty entries = so.FindProperty("_entries");
        foreach ((string word, Color color) in Colors)
        {
            Object effect = EffectOf(entries, word);
            if (effect == null) continue;
            var eso = new SerializedObject(effect);
            SerializedProperty p = eso.FindProperty("_color");
            if (p == null) continue;
            p.colorValue = color;
            eso.ApplyModifiedProperties();
            EditorUtility.SetDirty(effect);
        }
    }

    private static void SetFlags(SerializedProperty m, string flags, string pact, string note)
    {
        var set = new HashSet<string>(flags.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries));

        int timing = set.Contains("every") ? 1 : set.Contains("last") ? 2 : set.Contains("tick") ? 3 : set.Contains("reflect") ? 4 : 0;
        int target = set.Contains("enemy") ? 1 : set.Contains("self") ? 2 : set.Contains("all") ? 3 : 0;

        m.FindPropertyRelative("_timing").enumValueIndex = timing;
        m.FindPropertyRelative("_target").enumValueIndex = target;
        m.FindPropertyRelative("_byDamage").boolValue = set.Contains("bydamage");
        m.FindPropertyRelative("_stack").boolValue = set.Contains("stack");
        m.FindPropertyRelative("_permanent").boolValue = set.Contains("permanent");
        m.FindPropertyRelative("_riderFirst").boolValue = set.Contains("first");
        m.FindPropertyRelative("_onlyIfBodyHit").boolValue = set.Contains("onlyhit");
        m.FindPropertyRelative("_riderVisualOnly").boolValue = set.Contains("visual");
        m.FindPropertyRelative("_bodyVisualOnly").boolValue = set.Contains("bodyvisual");
        m.FindPropertyRelative("_again").boolValue = set.Contains("again");
        m.FindPropertyRelative("_area").boolValue = set.Contains("area");
        m.FindPropertyRelative("_pierceShield").boolValue = set.Contains("pierce");
        m.FindPropertyRelative("_keepScars").boolValue = set.Contains("keepscars");
        m.FindPropertyRelative("_noSelfDamage").boolValue = set.Contains("nodmg");
        m.FindPropertyRelative("_arenaBuff").boolValue = set.Contains("arena");
        m.FindPropertyRelative("_shades").boolValue = set.Contains("shades");
        m.FindPropertyRelative("_useRiderColor").boolValue = true;
        m.FindPropertyRelative("_rainbow").boolValue = false;
        m.FindPropertyRelative("_riderEveryHit").boolValue = false;
        m.FindPropertyRelative("_playRiderSound").boolValue = true;
        m.FindPropertyRelative("_pact").enumValueIndex = (int)(BloodPerk)System.Enum.Parse(typeof(BloodPerk), pact);
        m.FindPropertyRelative("_design").stringValue = note;
    }

    // overwrite = false: only adds the mixes the book does not have yet.
    // overwrite = true: also rewrites the rules (and the body/rider order) of the mixes it already has.
    // Returns (added, updated).
    internal static (int Added, int Updated) Apply(SpellBook book, bool overwrite)
    {
        if (book == null) return (0, 0);
        ApplyColors(book);

        int added = 0, updated = 0;
        var so = new SerializedObject(book);
        SerializedProperty entries = so.FindProperty("_entries");
        SerializedProperty mixes = so.FindProperty("_mixes");

        foreach ((string bodyWord, string riderWord, string name, string flags, string pact, string note) in Table)
        {
            Object body = EffectOf(entries, bodyWord);
            Object rider = EffectOf(entries, riderWord);
            if (body == null || rider == null) continue;

            int found = -1;
            for (int i = 0; i < mixes.arraySize; i++)
            {
                SerializedProperty m = mixes.GetArrayElementAtIndex(i);
                Object b = m.FindPropertyRelative("_body").objectReferenceValue;
                Object r = m.FindPropertyRelative("_rider").objectReferenceValue;
                if ((b == body && r == rider) || (b == rider && r == body)) { found = i; break; }
            }

            if (found >= 0 && !overwrite) continue;

            SerializedProperty target;
            if (found >= 0) { target = mixes.GetArrayElementAtIndex(found); updated++; }
            else
            {
                int n = mixes.arraySize;
                mixes.InsertArrayElementAtIndex(n);
                target = mixes.GetArrayElementAtIndex(n);
                added++;
            }

            target.FindPropertyRelative("_body").objectReferenceValue = body;
            target.FindPropertyRelative("_rider").objectReferenceValue = rider;
            target.FindPropertyRelative("_name").stringValue = name;
            target.FindPropertyRelative("_tint").colorValue = Color.white;
            SetFlags(target, flags, pact, note);
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(book);
        return (added, updated);
    }
}
