%  sim_norm_model
%
%  Initial version of the code for the normative model of the SpaceShoot task
%  built by Basile Garcia <basile.garcia@gmail.com>
%
%  Next step: fit a simplified normative model where the option value learning
%  component only learns block difficulty (reset at the beginning of each block)
%  and the forcefield destroyability learning component only learns the slope of
%  the logistic function (*not* reset at the beginning of each block). This
%  would dramatically increase the speed of the code, and makes quite standard
%  assumptions about what the normative model knows (rather than learns) about
%  the task:
%    1/ that there is a good and a bad option in each block (we could even tell
%       explicitly participants about it if necessary),
%    2/ that the forcefield destroyability function crosses p = 0.5 in the
%       middle of the color range (we could even tell explicitly participants
%       about it if necessary).
%
%  This simplified normative model will also include additional parameters to
%  make it suboptimal: exponential discounting of past rewards (suboptimal
%  option value learning), and exponential discounting of past forcefield
%  observations (suboptimal forcefield destroyability learning). This will allow
%  us to fit these parameters, along with the inverse temperatures associated
%  with the decision policy, to participant data.
%
%  Valentin Wyart <valentin.wyart@inserm.fr> | 2023-08-29

%% Simulate Kalman filter to model learning of option values
%
%  This code purposefully ignores forcefields (by assuming the absence of
%  forcefields) to focus on the learning of option values.
%
%  In this very simple piece of code, the model uses Thompson sampling to choose
%  probabilistically between the two options as a function of their respective
%  posterior means (mt) and variances (vt).
%
%  To-do: use the reparameterization function from drift variance (vd) to
%  asymptotic learning rate (alpha) to make the parameter more readily
%  interpretable in RL-like terms.

% clear workspace
clear all
close all
clc

% set task parameters
mrwd = [0.6,0.4]; % reward means
vrwd = 0.2^2;     % reward variance
nb   = 1e4;       % number of blocks
nt   = 50;        % number of trials per block

% set model parameters
m0   = 0.5;   % prior mean
v0   = 0.2^2; % prior variance
vs   = vrwd;  % sampling variance (scaling parameter, do not change)
vd   = 0;     % drift variance (optimal=0)
beta = 1e6;   % Thompson sampling inverse temperature

% sample rewards
xrwd = normrnd(repmat(reshape(mrwd,[1,1,2]),[nb,nt,1]),sqrt(vrwd));

mt   = nan(nb,nt,2); % posterior means
vt   = nan(nb,nt,2); % posterior variances
resp = nan(nb,nt);   % response
r_ch = nan(nb,nt);   % chosen reward
r_un = nan(nb,nt);   % unchosen/foregone reward

% initialize posterior means and variances
mt(:,1,:) = m0;
vt(:,1,:) = v0;

% get response
resp(:,1) = ceil(2*rand(nb,1));

% get chosen and unchosen/foregone rewards
r_ch(:,1) = xrwd(sub2ind(size(xrwd),(1:nb)',ones(nb,1),resp(:,1)));
r_un(:,1) = xrwd(sub2ind(size(xrwd),(1:nb)',ones(nb,1),3-resp(:,1)));

for it = 2:nt
    
    % compute Kalman gain
    kgain = vt(:,it-1,:)./(vt(:,it-1,:)+vs);
    
    % get blocks where chosen option = 1
    i1 = resp(:,it-1) == 1 & ~isnan(xrwd(:,it-1,1));
    
    % update posterior mean and variance of chosen option
    mt(i1,it,1) = mt(i1,it-1,1)+kgain(i1,1,1).*(xrwd(i1,it-1,1)-mt(i1,it-1,1));
    vt(i1,it,1) = (1-kgain(i1,1,1)).*vt(i1,it-1,1);
    % freeze posterior mean and variance of unchosen option
    mt(i1,it,2) = mt(i1,it-1,2);
    vt(i1,it,2) = vt(i1,it-1,2);
    
    % get blocks where chosen option = 2
    i2 = resp(:,it-1) == 2 & ~isnan(xrwd(:,it-1,1));
    
    % update posterior mean and variance of chosen option
    mt(i2,it,2) = mt(i2,it-1,2)+kgain(i2,1,2).*(xrwd(i2,it-1,2)-mt(i2,it-1,2));
    vt(i2,it,2) = (1-kgain(i2,1,2)).*vt(i2,it-1,2);
    % freeze posterior mean and variance of unchosen option
    mt(i2,it,1) = mt(i2,it-1,1);
    vt(i2,it,1) = vt(i2,it-1,1);
    
    % account for drift
    vt(:,it,:) = vt(:,it,:)+vd;
    
    % get log-likelihood ratio
    llr = 2*(mt(:,it,1)-mt(:,it,2))./sqrt(vt(:,it,1)+vt(:,it,2));
    
    % get response
    resp(:,it) = 1+(rand(nb,1) > 1./(1+exp(-beta*llr)));
    
    % get chosen and unchosen/foregone rewards
    r_ch(:,it) = xrwd(sub2ind(size(xrwd),(1:nb)',it(ones(nb,1)),resp(:,it)));
    r_un(:,it) = xrwd(sub2ind(size(xrwd),(1:nb)',it(ones(nb,1)),3-resp(:,it)));
    
end

% compute statistics of option values at each trial position across blocks
mt_avg = squeeze(mean(mt,1));
mt_std = squeeze(std(mt,[],1));

figure;

% plot chosen option over time
subplot(2,1,1);
hold on
xlim([0.5,nt+0.5]);
ylim([0,1]);
plot(xlim,[0.5,0.5],'-','LineWidth',0.75,'Color',[0.8,0.8,0.8]);
plot(1:nt,mean(resp == 1,1),'k-','LineWidth',2);
hold off
set(gca,'TickDir','out');
xlabel('trial position');
ylabel('p(choose option1)');

% plot learnt option values over time
subplot(2,1,2);
hold on
xlim([0.5,nt+0.5]);
ylim([0,1]);
patch([(1:nt)';flip((1:nt)')],[mt_avg(:,1)+mt_std(:,1);flip(mt_avg(:,1)-mt_std(:,1))],[0,0.75,0],'EdgeColor','none','FaceAlpha',0.25);
patch([(1:nt)';flip((1:nt)')],[mt_avg(:,2)+mt_std(:,2);flip(mt_avg(:,2)-mt_std(:,2))],[1,0.5,0],'EdgeColor','none','FaceAlpha',0.25);
plot(1:nt,mt_avg(:,1),'-','LineWidth',2,'Color',[0,0.75,0]);
plot(1:nt,mt_avg(:,2),'-','LineWidth',2,'Color',[1,0.5,0]);
hold off
set(gca,'TickDir','out');
xlabel('trial position');
ylabel('option value');

sgtitle('Kalman filter');

%% Simulate logistic regression to model learning of forcefield destroyability
%
%  This code purposefully ignores options (by assuming that option1 is chosen on
%  every trial) to focus on the learning of forcefield destroyability.
%
%  In this very simple piece of code, we assume that the model re-learns
%  forcefield destroyability from scratch at the beginning of each new block.
%
%  To-do: create a new logistic regression function with optional exponential
%  discounting of past observations to account for perceptual memory decay.

% clear workspace
clear all
close all
clc

% set task parameters
bffd = [0,1]; % forcefield parameter estimates
nb   = 1e2;   % number of blocks
nt   = 50;    % number of trials per block

% set model parameters
vffd = [2,2]; % prior variances on forcefield parameter estimates 

% sample forcefields and their destroyability upon impact
xffd = unifrnd(-1,+1,[nb,nt,2]);
yffd = rand(size(xffd)) < 1./(1+exp(-bffd(1)-bffd(2)*xffd));

breg = nan(nb,nt,2); % forcefield parameter estimates

hbar = waitbar(0,'running logistic regression model');
for ib = 1:nb
    waitbar(ib/nb,hbar);
    for it = 1:nt
        % by default, assume that option1 is chosen on every trial
        xreg = [ones(it,1),xffd(ib,1:it,1)'];
        yreg = yffd(ib,1:it,1)';
        breg(ib,it,:) = logreg_regul(xreg,yreg,'b_sigma',vffd,'nrun',1);
    end
end
close(hbar);

% compute statistics of forcefield parameter estimates at each trial position across blocks
breg_avg = squeeze(mean(breg,1));
breg_std = squeeze(std(breg,[],1));

% compute predicted forcefield curves over time
xvec = -1:0.01:+1;
pvec = nan(numel(xvec),nt);
for it = 1:nt
    pvec(:,it) = 1./(1+exp(-breg_avg(it,1)-breg_avg(it,2)*xvec));
end

figure;

% plot forcefield parameter estimates
subplot(2,1,1);
hold on
xlim([0.5,nt+0.5]);
patch([(1:nt)';flip((1:nt)')],[breg_avg(:,1)+breg_std(:,1);flip(breg_avg(:,1)-breg_std(:,1))],[0,0.5,0.5],'EdgeColor','none','FaceAlpha',0.25);
patch([(1:nt)';flip((1:nt)')],[breg_avg(:,2)+breg_std(:,2);flip(breg_avg(:,2)-breg_std(:,2))],[0,1,0.5],'EdgeColor','none','FaceAlpha',0.25);
plot(1:nt,breg_avg(:,1),'-','LineWidth',2,'Color',[0,0.5,0.5]);
plot(1:nt,breg_avg(:,2),'-','LineWidth',2,'Color',[0,1,0.5]);
plot(xlim,bffd([1,1]),'k--','LineWidth',0.75);
plot(xlim,bffd([2,2]),'k--','LineWidth',0.75);
hold off
set(gca,'TickDir','out');
xlabel('trial position');
ylabel('forcefield parameter estimates');

% plot predicted forcefield curves over time
subplot(2,1,2);
imagesc(1:nt,xvec,pvec);
hold on
xlim([0.5,nt+0.5]);
ylim([-1,+1]);
plot(xlim,[0,0],'k--','LineWidth',0.75);
hold off
set(gca,'TickDir','out','Box','off','CLim',[0,1],'YDir','normal');
xlabel('trial position');
ylabel('forcefield color');

sgtitle('Logistic regression model');

%% Simulate both components of the normative model at once
%
%  Here we assume that the model re-learns option values at the beginning of
%  each new block, but only updates its learnt parameters estimates of
%  forcefield destroyability.
%
%  On each trial, using the posterior means of the Kalman filter (mt) and the
%  parameter estimates of forcefield destroyability applied to the current
%  forcefield colors (pt), the model computes expected values (EV) as mt*pt for
%  the two options, and chooses between them with an adjustable sampling
%  temperature (zero for argmax).
%
%  At the moment, it is easy to make the option value learning component of the
%  model suboptimal, but setting vd > 0 (the model then discounts exponentially
%  past rewards). But it is not possible to make the forcefield destroyability
%  learning component of the model suboptimal. I should create a dedicated
%  logreg_regul function where the log-likelihood objective function of the
%  regression model can discount past observations.
%
%  This should be done have parameters to fit, the only problem being that the
%  current implementation is way too slow in terms of the logistic regression
%  model. There is no closed-loop analytical function for fitting the forcefield
%  destroyability sigmoid model unfortunately, but maybe I could work out some
%  sort of approximation that would work well (to be continued).
%
%  Note that the number of trials per block (nt) should be set so that we have
%  enough trials per block to reach the learning asymptote and have some trials
%  after the asymptote is reached, but not too many because it is really the
%  learning aspect that we care mostly about.

% clear workspace
clear all
close all
clc

% set task parameters
bffd = [0,1.25];  % forcefield parameter estimates
mrwd = [0.6,0.4]; % reward means
vrwd = 0.2^2;     % reward variance
nb   = 1e3;       % number of blocks (use 1e3 as default)
nt   = 25;        % number of trials per block

% set model parameters
vffd    = [2,2]; % prior variances on forcefield parameter estimates 
m0      = 0.5;   % prior mean
v0      = 0.2^2; % prior variance
vs      = vrwd;  % sampling variance (scaling parameter)
vd      = 0;     % drift variance
dvtype  = 'ev';  % decision variable type ('ev' or 'lr')
beta_ev = 1e6;   % inverse temperature for expected values (if dvtype == 'ev')
beta_m  = 1e6;   % inverse temperature for forcefield destroyabilities (if dvtype == 'lr')
beta_p  = 1e6;   % inverse temperature for option values (if dvtype == 'lr')

% sample forcefields and their destroyability upon impact
xffd = unifrnd(-1,+1,[nb,nt,2]);
yffd = rand(size(xffd)) < 1./(1+exp(-bffd(1)-bffd(2)*xffd));

% sample rewards
xrwd = normrnd(repmat(reshape(mrwd,[1,1,2]),[nb,nt,1]),sqrt(vrwd));

breg   = nan(nb,nt,2); % forcefield parameter estimates
pt     = nan(nb,nt,2); % forcefield destroyabilities
mt     = nan(nb,nt,2); % posterior means
vt     = nan(nb,nt,2); % posterior variances
ropt   = nan(nb,nt);   % optimal response
ropt_m = nan(nb,nt);   % optimal response wrt option values
ropt_p = nan(nb,nt);   % optimal response wrt forcefield destroyabilities
resp   = nan(nb,nt);   % model response
x_ch   = nan(nb,nt);   % chosen forcefield color
y_ch   = nan(nb,nt);   % chosen forcefield destroyed?
r_ch   = nan(nb,nt);   % chosen reward
r_un   = nan(nb,nt);   % unchosen/foregone reward

hbar = waitbar(0,'running normative model');
for ib = 1:nb
    waitbar(ib/nb,hbar);
    
    % compute true forcefield destroyabilities and true option values
    ptru = squeeze(1./(1+exp(-bffd(1)-bffd(2)*xffd(ib,1,:))));
    mtru = mrwd(:);
    % compute optimal response as argmax on optimal expected values
    [~,ropt(ib,1)] = max(mtru.*ptru);
    [~,ropt_m(ib,1)] = max(mtru); % wrt option values only
    [~,ropt_p(ib,1)] = max(ptru); % wrt forcefield destroyabilities only

    if ib > 1
        % compute forcefield destroyabilities
        pt(ib,1,:) = 1./(1+exp(-breg(ib-1,nt,1)-breg(ib-1,nt,2)*xffd(ib,1,:)));
    end
    
    % initialize posterior means and variances
    mt(ib,1,:) = m0;
    vt(ib,1,:) = v0;
    
    % get response
    if ib > 1
        % choose using forcefield destroyabilities
        switch dvtype
            case 'ev' % decision variable = difference of forcefield destroyabilities
                dv = beta_ev*(pt(ib,1,1)-pt(ib,1,2));
            case 'lr' % decision variable = log-ratio of forcefield destroyabilities
                dv = beta_p*(log(pt(ib,1,1))-log(pt(ib,1,2)));
            otherwise
                error('Undefined decision variable type!');
        end
        resp(ib,1) = 1+(rand > 1/(1+exp(-dv)));
    else
        % choose randomly
        resp(ib,1) = ceil(2*rand);
    end
    
    % check whether forcefield is destroyed
    x_ch(ib,1) = xffd(ib,1,resp(ib,1));
    y_ch(ib,1) = yffd(ib,1,resp(ib,1));
    
    % update forcefield parameter estimates
    xreg = x_ch(~isnan(x_ch(:)));
    xreg = cat(2,ones(size(xreg)),xreg);
    yreg = y_ch(~isnan(y_ch(:)));
    breg(ib,1,:) = logreg_regul(xreg,yreg,'b_sigma',vffd,'nrun',1);
    
    if y_ch(ib,1) == true
        % get chosen and unchosen/foregone rewards
        r_ch(ib,1) = xrwd(ib,1,resp(ib,1));
        r_un(ib,1) = xrwd(ib,1,3-resp(ib,1));
    end
    
    for it = 2:nt
        
        % compute true forcefield destroyabilities and true option values
        ptru = squeeze(1./(1+exp(-bffd(1)-bffd(2)*xffd(ib,it,:))));
        mtru = mrwd(:);
        % compute optimal response
        [~,ropt(ib,it)] = max(mtru.*ptru); % wrt expected values
        [~,ropt_m(ib,it)] = max(mtru); % wrt option values only
        [~,ropt_p(ib,it)] = max(ptru); % wrt forcefield destroyabilities only
        
        % compute forcefield destroyabilities
        pt(ib,it,:) = 1./(1+exp(-breg(ib,it-1,1)-breg(ib,it-1,2)*xffd(ib,it,:)));
        
        ch = resp(ib,it-1); % chosen option on previous trial
        un = 3-ch; % unchosen option on previous trial
        
        % compute Kalman gain
        kgain = vt(ib,it-1,:)./(vt(ib,it-1,:)+vs);
        
        if y_ch(ib,it-1) == true
            % update posterior mean and variance of chosen option
            mt(ib,it,ch) = mt(ib,it-1,ch)+kgain(1,1,ch).*(xrwd(ib,it-1,ch)-mt(ib,it-1,ch));
            vt(ib,it,ch) = (1-kgain(1,1,ch)).*vt(ib,it-1,ch);
        else
            % freeze posterior mean and variance of chosen option
            mt(ib,it,ch) = mt(ib,it-1,ch);
            vt(ib,it,ch) = vt(ib,it-1,ch);
        end
        % freeze posterior mean and variance of unchosen option
        mt(ib,it,un) = mt(ib,it-1,un);
        vt(ib,it,un) = vt(ib,it-1,un);
        
        % account for drift
        vt(ib,it,:) = vt(ib,it,:)+vd;
        
        % compute decision variable
        switch dvtype
            case 'ev' % decision variable = difference of expected values
                % compute expected values
                ev = squeeze(mt(ib,it,:).*pt(ib,it,:));
                % compute decision variable
                dv = beta_ev*(ev(1)-ev(2));
            case 'lr' % decision variable = weighted sum of log-ratios
                % compute log-ratio of option values
                logm = squeeze(log(mt(ib,it,:)));
                lr_m = logm(1)-logm(2);
                % compute log-ratio of forcefield destroyabilities
                logp = squeeze(log(pt(ib,it,:)));
                lr_p = logp(1)-logp(2);
                % compute decision variable
                dv = beta_m*lr_m+beta_p*lr_p;
            otherwise
                error('Undefined decision variable type!');
        end
        
        % get response using softmax policy on decision variable
        resp(ib,it) = 1+(rand > 1/(1+exp(-dv)));
        
        % check whether forcefield is destroyed
        x_ch(ib,it) = xffd(ib,it,resp(ib,it));
        y_ch(ib,it) = yffd(ib,it,resp(ib,it));
        
        % update forcefield parameter estimates
        xreg = x_ch(~isnan(x_ch(:)));
        xreg = cat(2,ones(size(xreg)),xreg);
        yreg = y_ch(~isnan(y_ch(:)));
        breg(ib,it,:) = logreg_regul(xreg,yreg,'b_sigma',vffd,'nrun',1);
        
        if y_ch(ib,it) == true
            % get chosen and unchosen/foregone rewards
            r_ch(ib,it) = xrwd(ib,it,resp(ib,it));
            r_un(ib,it) = xrwd(ib,it,3-resp(ib,it));
        end
        
    end
    
end
close(hbar);

%% Plot task information

figure;

% plot reward distributions
subplot(1,2,1);
hold on
xlim([0,1]);
x = 0.01:0.01:0.99;
patch([x,flip(x)],[normpdf(x,mrwd(1),sqrt(vrwd)),zeros(size(x))],[0,0.75,0],'FaceAlpha',0.25,'EdgeColor','none');
patch([x,flip(x)],[normpdf(x,mrwd(2),sqrt(vrwd)),zeros(size(x))],[1,0.5,0],'FaceAlpha',0.25,'EdgeColor','none');
plot(x,normpdf(x,mrwd(1),sqrt(vrwd)),'-','LineWidth',2,'Color',[0,0.75,0]);
plot(x,normpdf(x,mrwd(2),sqrt(vrwd)),'-','LineWidth',2,'Color',[1,0.5,0]);
ylim([0,max(ylim)]);
plot(mrwd([1,1]),ylim,'--','LineWidth',0.75,'Color',[0,0.75,0]);
plot(mrwd([2,2]),ylim,'--','LineWidth',0.75,'Color',[1,0.5,0]);
hold off
set(gca,'TickDir','out','XTick',0:0.2:1,'YTick',0);
xlabel('reward');
ylabel('probability density');

% plot forcefield destroyability function
subplot(1,2,2);
hold on
xlim([-1,+1]);
ylim([0,1]);
x = -1:0.01:+1;
patch([x,flip(x)],[1./(1+exp(-bffd(1)-bffd(2)*x)),zeros(size(x))],[0.5,0.5,0.5],'FaceAlpha',0.25,'EdgeColor','none');
plot(x,1./(1+exp(-bffd(1)-bffd(2)*x)),'k-','LineWidth',2);
hold off
set(gca,'TickDir','out','XTick',-1:0.5:+1,'YTick',0:0.2:1);
xlabel('forcefield color');
ylabel('forcefield destroyability');

sgtitle('Task information');

%% Plot response optimality
%
%  Legend of plotted curves:
%    * gray = optimal wrt expected values
%    * red  = optimal wrt forcefield destroyabilities
%    * blue = optimal wrt option values
%
%  What we want to achieve is that the forcefield destroyability information and
%  the option value information are equally uncertain, meaning that response
%  optimality is matched wrt forcefield destroyabilities and wrt option values.
%  This is achieved by bffd = [0,1.25] and mrwd = [0.6,0.4], vrwd = 0.2^2.

% fit binomial distributions to simulated responses
rhat   = nan(1,nt);
r_ci   = nan(2,nt);
rhat_p = nan(1,nt);
r_ci_p = nan(2,nt);
rhat_m = nan(1,nt);
r_ci_m = nan(2,nt);
for it = 1:nt
    [rhat(it),r_ci(:,it)] = binofit(nnz(resp(:,it) == ropt(:,it)),nb);
    [rhat_p(it),r_ci_p(:,it)] = binofit(nnz(resp(:,it) == ropt_p(:,it)),nb);
    [rhat_m(it),r_ci_m(:,it)] = binofit(nnz(resp(:,it) == ropt_m(:,it)),nb);
end

% plot response optimality
figure;
hold on
xlim([0.5,nt+0.5]);
ylim([0.4,1]);
patch([1:nt,flip(1:nt)],[r_ci(1,:),flip(r_ci(2,:))],[0.5,0.5,0.5],'FaceAlpha',0.25,'EdgeColor','none');
patch([1:nt,flip(1:nt)],[r_ci_p(1,:),flip(r_ci_p(2,:))],[1,0,0],'FaceAlpha',0.25,'EdgeColor','none');
patch([1:nt,flip(1:nt)],[r_ci_m(1,:),flip(r_ci_m(2,:))],[0,0,1],'FaceAlpha',0.25,'EdgeColor','none');
plot(xlim,[0.5,0.5],'-','LineWidth',0.75,'Color',[0.8,0.8,0.8]);
plot(1:nt,rhat,'-','LineWidth',2,'Color',[0.5,0.5,0.5]);
plot(1:nt,rhat_p,'-','LineWidth',2,'Color','r');
plot(1:nt,rhat_m,'-','LineWidth',2,'Color','b');
hold off
set(gca,'TickDir','out','XTick',5:5:25,'YTick',0:0.2:1);
xlabel('trial position');
ylabel('p(optimal)');

sgtitle('Response optimality');
